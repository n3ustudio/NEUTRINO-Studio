using System;
using System.Collections.Generic;
using System.Configuration;
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

        private static readonly Configuration Config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        /// <summary>
        /// Get app setting.
        /// </summary>
        /// <param name="key">Key of the setting.</param>
        /// <returns></returns>
        public static string GetAppSetting(string key)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Contains(key))
            {
                string value = Config.AppSettings.Settings[key].Value;
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
        public static void UpdateAppSetting(string key, string value)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Contains(key))
            {
                Config.AppSettings.Settings[key].Value = value;
                Config.Save(ConfigurationSaveMode.Modified);
            }
            else
            {
                Config.AppSettings.Settings.Add(key, value);
                Config.Save(ConfigurationSaveMode.Modified);
            }
        }
    }
}
