using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace NeutrinoStudio.Shell.Helpers
{
    /// <summary>
    /// Config Helper.
    /// </summary>
    public static class ConfigHelper
    {

        private static Config _current = OpenConfig();

        /// <summary>
        /// The current config.
        /// </summary>
        public static Config Current
        {
            get => _current;
            set => _current = value;
        }

        public static readonly string UserDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Il Harper\\Neutrino Studio");

        private static Config OpenConfig()
        {
            Directory.CreateDirectory(UserDataFolder);
            FileStream fs = new FileStream(
                Path.Combine(UserDataFolder, "config.dat"), FileMode.OpenOrCreate, FileAccess.Read,
                FileShare.ReadWrite);
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                Config config = formatter.Deserialize(fs) as Config;
                fs.Close();
                return config;
            }
            catch (SerializationException)
            {
                fs.Close();
                return new Config();
            }
        }

        /// <summary>
        /// Save the config.
        /// </summary>
        public static void SaveConfig()
        {
            FileStream fs = new FileStream(
                Path.Combine(UserDataFolder, "config.dat"), FileMode.OpenOrCreate, FileAccess.ReadWrite,
                FileShare.ReadWrite);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(fs, Current);
            fs.Close();
        }

    }

    [Serializable]
    public class Config
    {

        private string _neutrinoDir = null;

        public string NeutrinoDir
        {
            get => _neutrinoDir;
            set => _neutrinoDir = value;
        }

    }
}
