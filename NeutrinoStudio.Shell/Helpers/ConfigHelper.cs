using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutrinoStudio.Shell.Helpers
{
    /// <summary>
    /// Config Helper.
    /// </summary>
    public static class ConfigHelper
    {
        /// <summary>
        /// Get app setting.
        /// </summary>
        /// <param name="key">Key of the setting.</param>
        /// <returns></returns>
        public static string GetAppSetting(string key)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Contains(key))
            {
                string value = config.AppSettings.Settings[key].Value;
                return value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Update app setting.
        /// </summary>
        /// <param name="key">Key of the setting.</param>
        /// <param name="value">Value of the setting.</param>
        public static void UpdateAppSettings(string key, string value)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Contains(key))
            {
                config.AppSettings.Settings[key].Value = value;
                config.Save(ConfigurationSaveMode.Modified);
            }
            else
            {
                config.AppSettings.Settings.Add(key, value);
                config.Save(ConfigurationSaveMode.Modified);
            }
        }
    }
}
