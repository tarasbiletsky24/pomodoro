﻿using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

// #laterusable
namespace PomodoroTimer.Converters
{
    public class HideIfEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                var stringValue = (string)value;

                if (String.IsNullOrEmpty(stringValue))
                    return false;
                else
                    return true;
            }
            catch
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
