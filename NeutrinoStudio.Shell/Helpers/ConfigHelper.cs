using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutrinoStudio.Shell.Helpers
{
    public static class ConfigHelper
    {
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

        public static void UpdateAppSettings(string key, string value)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Contains(key))
            {
                //如果当前节点存在，则更新当前节点
                config.AppSettings.Settings[key].Value = value;
                config.Save(ConfigurationSaveMode.Modified);
            }
            else
            {
                Console.WriteLine("当前节点不存在");
            }
        }
    }
}
