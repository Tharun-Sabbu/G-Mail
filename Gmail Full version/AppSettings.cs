using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO.IsolatedStorage;
using System.Diagnostics;
using System.Collections.Generic;

namespace Gmail_Full_version
{
    public class AppSettings
    {
        IsolatedStorageSettings settings;
        string CheckBoxSettingKeyName = "CheckBoxSetting1";
        string ToggleButtonKeyName = "ToggleButton";
        string ToggleButtonKeyName1 = "ToggleButton1";
        const bool CheckBoxSettingDefault = true;
        const bool ToggleButtonSettingDefault = true;
        public AppSettings()
        {
            try
            {
                settings = IsolatedStorageSettings.ApplicationSettings;
            }
            catch (System.IO.IsolatedStorage.IsolatedStorageException e)
            {
                e.GetType();

            }
        }
        public bool AddOrUpdateValue(string Key, Object value)
        {
            bool valueChanged = false;
            if (settings.Contains(Key))
            {
                if (settings[Key] != value)
                {
                    settings[Key] = value;
                    valueChanged = true;
                }
            }
            else
            {
                settings.Add(Key, value);
                valueChanged = true;
            }
            return valueChanged;
        }
        public T GetValueOrDefault<T>(string Key, T defaultValue)
        {
            T value;
            if (settings.Contains(Key))
            {
                value = (T)settings[Key];
            }
            else
            {
                value = defaultValue;
            }
            return value;
        }
        public void save()
        {
            settings.Save();
        }
        public bool CheckBoxSetting1
        {
            get
            {
                return GetValueOrDefault<bool>(CheckBoxSettingKeyName, CheckBoxSettingDefault);
            }
            set
            {
                if (AddOrUpdateValue(CheckBoxSettingKeyName, value))
                {
                    save();
                }
            }
        }
        public bool ToggleButton
        {
            get
            {
                return GetValueOrDefault<bool>(ToggleButtonKeyName, ToggleButtonSettingDefault);
            }
            set
            {
                if (AddOrUpdateValue(ToggleButtonKeyName, value))
                {
                    save();
                }
            }
        }

        public bool ToggleButton1
        {
            get
            {
                return GetValueOrDefault<bool>(ToggleButtonKeyName1, ToggleButtonSettingDefault);
            }
            set
            {
                if (AddOrUpdateValue(ToggleButtonKeyName1, value))
                {
                    save();
                }
            }
        }
    }
}
