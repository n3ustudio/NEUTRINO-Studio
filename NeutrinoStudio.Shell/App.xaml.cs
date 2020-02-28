using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using MetroRadiance.UI;
using NeutrinoStudio.Shell.Helpers;

namespace NeutrinoStudio.Shell
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // Initialize
            LogHelper.Current.Log(LogType.Info, $"NEUTRINO Studio {Assembly.GetExecutingAssembly().GetName().Version}");

            // Show Window
            if (MainWindow is null) MainWindow = new MainWindow();
            MainWindow.Show();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ThemeService.Current.Register(this, Theme.Windows, Accent.Windows);
        }
    }
}
