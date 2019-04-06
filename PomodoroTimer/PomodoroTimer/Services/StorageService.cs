﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;
using PomodoroTimer.Models;
using PomodoroTimer.Utils;
using Xamarin.Essentials;
namespace PomodoroTimer.Services
{
    public class StorageModel
    {
        public AplicationUser User { get; set; }
        public AppSettings AppSettings { get; set; }
        public List<PomodoroSession> Sessions { get; set; } = new List<PomodoroSession>();
        public List<UserTask> UserTasks { get; set; } = new List<UserTask>();
        public PomdoroStatus AppState { get; internal set; }
    }

    public interface IPreferencesService
    {
        Task<bool> SaveAsync();
        Task<bool> LoadAsync();
        bool Save();
        bool Load();
        void Clear();
    }

    public abstract class PreferencesServiceBase<TPreferenceModel> : IPreferencesService
    {
        public TPreferenceModel StorageModel { get; set; }

        public Task<bool> SaveAsync()
        {
            return Task.Run(() => { return Save(); });
        }
        public Task<bool> LoadAsync()
        {
            return Task.Run(() => { return Load(); });
        }
        public bool Save()
        {
            try
            {
                var jsonString = JsonConvert.SerializeObject(StorageModel);
                Preferences.Set("DataStore", jsonString);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Load()
        {
            try
            {
                var dataString = Preferences.Get("DataStore", string.Empty);
                StorageModel = JsonConvert.DeserializeObject<TPreferenceModel>(dataString);
                return StorageModel != null;
            }
            catch
            {
                return false;
            }
        }
        public void Clear()
        {
            Preferences.Set("DataStore", "");
        }
    }

    public class StorageService : PreferencesServiceBase<StorageModel>, IStorageService
    {
        public StorageService()
        {
            if (!Load())
                StorageModel = new StorageModel();
        }

        public List<UserTask> GetAllUserTask(AplicationUser user)
        {
            //if default tasks not contain default_user_task  add it
            if (!StorageModel.UserTasks.Exists(x => x.Id == AppConstants.DEFAULT_USER_TASK.Id))
            {
                StorageModel.UserTasks.Add(AppConstants.DEFAULT_USER_TASK);
                Save();
            }
            GetStatistics();
            return StorageModel.UserTasks;

        }

        public AplicationUser GetUser()
        {
            return StorageModel.User;
        }

        public PomodoroSession GetSession()
        {
            var todaySession = StorageModel.Sessions.SingleOrDefault((x) => x.Day == DateTime.Today);
            if (todaySession == null)
            {
                todaySession = new PomodoroSession() { Day = DateTime.Today };
                StorageModel.Sessions.Add(todaySession);
            }

            return todaySession;
        }

        public Task<bool> RemoveUserTask(UserTask userTask)
        {
            if (userTask == null)
            {
                return Task.FromResult(false);
            }
            // not delete user task 
            if (userTask.Id == AppConstants.DEFAULT_USER_TASK.Id)
                return Task.FromResult(false);

            StorageModel.UserTasks.RemoveAll(x => x.Id == userTask.Id);

            return SaveAsync();
        }

        public AppSettings GetAppSettings()
        {
            return StorageModel.AppSettings;
        }

        public Task<bool> UpdateUserTask(UserTask userTask)
        {
            if (userTask == null)
                return Task.FromResult(false);

            var index = StorageModel.UserTasks.FindIndex(x => x.Id == userTask.Id);
            if (index >= 0)
                StorageModel.UserTasks[index] =userTask;

            return SaveAsync();
        }

        private void GetStatistics()
        {
            var allFinishedTask = StorageModel.Sessions.SelectMany(x => x.FinishedTaskInfo);
            var yearlyFinishedTask = allFinishedTask.Where(x => x.FinishedTime.Year == DateTime.Now.Year);
            var monthlyFinishedTask = yearlyFinishedTask.Where(x => x.FinishedTime.Month == DateTime.Now.Month);
            var weeklyFinishedTask = allFinishedTask.Where(x => x.FinishedTime.Iso8601WeekOfYear() == DateTime.Now.Iso8601WeekOfYear());
            var dailyFinishedTask = weeklyFinishedTask.Where(x => x.FinishedTime.DayOfYear == DateTime.Now.DayOfYear);

            foreach (var userTask in StorageModel.UserTasks)
            {
                var taskStatistic = new FinishedTaskStatistic
                {
                    DailyFinishedCount = dailyFinishedTask.Where(x => x.TaskId == userTask.Id)?.Count() ?? 0,
                    WeeklyFinishedCount = weeklyFinishedTask.Where(x => x.TaskId == userTask.Id)?.Count() ?? 0,
                    MonthlyFinishedCount = monthlyFinishedTask.Where(x => x.TaskId == userTask.Id)?.Count() ?? 0,
                    YearlyFinishedCount = yearlyFinishedTask.Where(x => x.TaskId == userTask.Id)?.Count() ?? 0
                };
                userTask.TaskStatistic = taskStatistic;
            }
        }
        public PomdoroStatus GetLastState()
        {
            return StorageModel.AppState;
        }

        public List<TaskStatistic> GetStatisticData(DateTime startTime, DateTime finishTime)
        {
            List<TaskStatistic> statistics = new List<TaskStatistic>();

            foreach (var session in StorageModel.Sessions)
            {
                if (session.Day >= startTime && session.Day <= finishTime)
                {
                    if (session.FinishedTaskInfo != null)
                        statistics.AddRange(session.FinishedTaskInfo);
                }
            }

            return statistics;
        }

        public Task<bool> AddNewUserTask(UserTask userTask)
        {
            if (StorageModel.UserTasks == null)
            {
                StorageModel.UserTasks = new List<UserTask>();
            }

            if (StorageModel.UserTasks.FirstOrDefault(x => x.Id == userTask.Id) == null)
            {
                StorageModel.UserTasks.Add(userTask);
            }
            else
            {
                StorageModel.UserTasks.RemoveAll(x => x.Id == userTask.Id);
                StorageModel.UserTasks.Add(userTask);
            }
            return SaveAsync();
        }

        public Task<bool> ClearStatistics(DateTime startTime, DateTime finishTime)
        {
            StorageModel.Sessions.RemoveAll(session => session.Day >= startTime && session.Day <= finishTime);
            return SaveAsync();
        }

        public Task<bool> SetAppSettings(AppSettings settings)
        {
            if (settings == null)
                return Task.FromResult(false);

            StorageModel.AppSettings = settings;

            return SaveAsync();
        }

        public Task<bool> UpdateSessionInfo(PomodoroSession currentSession)
        {
            var preValue = StorageModel.Sessions.SingleOrDefault((x) => x.Id == currentSession.Id);

            StorageModel.Sessions.RemoveAll((x) => x.Id == currentSession.Id);

            StorageModel.Sessions.Add(currentSession);

            return SaveAsync();
        }

        public Task<bool> SaveAppState(PomdoroStatus appState)
        {
            StorageModel.AppState = appState;
            return SaveAsync();
        }
    }
}
