﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PomodoroTimer.Models;
using PomodoroTimer.Services;

namespace PomodoroTimer
{
    public class HalfMockStorageService : StorageService
    {
        public new List<TaskStatistic> GetStatisticData(DateTime startTime, DateTime finishTime)
        {
            List<TaskStatistic> statistics = new List<TaskStatistic>();

            var sessions = MockDataService.CreateStatisticData(5, 10, startTime, finishTime);

            foreach (var session in sessions)
            {
                if (session.Day >= startTime && session.Day <= finishTime)
                {
                    if (session.FinishedTaskInfo != null)
                        statistics.AddRange(session.FinishedTaskInfo);
                }
            }

            return statistics;
        }
    }

    public class MockStorageService : IStorageService
    {
        List<UserTask> UserTasks = new List<UserTask>();
        public bool AddNewUserTask(UserTask userTask)
        {
            UserTasks.Add(userTask);
            return true;
        }

        public List<UserTask> GetAllUserTask(AplicationUser user)
        {


            for (int i = 0; i < 100; i++)
                UserTasks.Add(MockDataService.CreateUserTask());

            return UserTasks;
        }

        public AppSettings GetAppSettings()
        {
            return new AppSettings();
        }

        public PomodoroSession GetSession()
        {
            throw new NotImplementedException();
        }

        public List<TaskStatistic> GetStatisticData(DateTime startTime, DateTime finishTime)
        {
            List<TaskStatistic> statistics = new List<TaskStatistic>();

            var dayCount = (finishTime - startTime).TotalDays;
            var userTasks = new List<UserTask>()
            {
                new UserTask()
                {
                    PomodoroSettings= new PomodoroSettings(){PomodoroDuration=TimeSpan.FromMinutes(25)},
                    TaskName="Task1",
                },
                new UserTask()
                {
                    PomodoroSettings= new PomodoroSettings(){PomodoroDuration=TimeSpan.FromMinutes(25)},
                    TaskName="Task2",
                },
                new UserTask()
                {
                    PomodoroSettings= new PomodoroSettings(){PomodoroDuration=TimeSpan.FromMinutes(25)},
                    TaskName="Task3",
                },
                new UserTask()
                {
                    PomodoroSettings= new PomodoroSettings(){PomodoroDuration=TimeSpan.FromMinutes(25)},
                    TaskName="Task4",
                }
            };
            var r = new Random();

            for (int d = 0; d < dayCount; d++)
            {
                foreach (var t in userTasks)
                {
                    var count = 10 + r.Next() % 10;
                    for (int i = 0; i < count; i++)
                    {
                        TaskStatistic s = new TaskStatistic();
                        s.TaskName = t.TaskName;
                        s.Duration = t.PomodoroSettings.PomodoroBreakDuration;
                        s.Id = new Guid();
                        s.TaskId = t.Id;
                        s.FinishedTime = startTime.AddDays(d);
                        statistics.Add(s);
                    }
                }
            }

            return statistics;
        }

        public AplicationUser GetUser()
        {
            var aplicationUser = new AplicationUser()
            {
                Id = new System.Guid(),
                Name = "Name",
                Pass = "Pass",
                Email = "user@user.com"

            };
            return aplicationUser;
        }

        public bool RemoveUserTask(UserTask userTask)
        {
            UserTasks.Remove(userTask);
            return true;
        }

        public bool SetAppSettings(AppSettings settings)
        {
            return true;
        }

        public bool UpdateSessionInfo(PomodoroSession currentSession)
        {
            throw new NotImplementedException();
        }

        public bool UpdateUserTask(UserTask task)
        {
            return true;
        }
        public bool ClearStatistics(DateTime startTime, DateTime finishTime)
        {
            return true;
        }

        public PomdoroStatus ReadLastState()
        {
            throw new NotImplementedException();
        }

        public void SaveAppState(PomdoroStatus appState)
        {
            throw new NotImplementedException();
        }

        public PomdoroStatus GetLastState()
        {
            throw new NotImplementedException();
        }

        Task<bool> IStorageService.UpdateUserTask(UserTask task)
        {
            throw new NotImplementedException();
        }

        Task<bool> IStorageService.AddNewUserTask(UserTask userTask)
        {
            throw new NotImplementedException();
        }

        Task<bool> IStorageService.RemoveUserTask(UserTask userTask)
        {
            throw new NotImplementedException();
        }

        Task<bool> IStorageService.SetAppSettings(AppSettings settings)
        {
            throw new NotImplementedException();
        }

        Task<bool> IStorageService.UpdateSessionInfo(PomodoroSession currentSession)
        {
            throw new NotImplementedException();
        }

        Task<bool> IStorageService.ClearStatistics(DateTime startTime, DateTime finishTime)
        {
            throw new NotImplementedException();
        }

        Task<bool> IStorageService.SaveAppState(PomdoroStatus appState)
        {
            throw new NotImplementedException();
        }
    }
}