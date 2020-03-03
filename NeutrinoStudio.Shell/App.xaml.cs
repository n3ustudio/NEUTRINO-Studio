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
using NeuTask;
using NeutrinoStudio.Shell.Helpers;
using TaskStatus = NeuTask.TaskStatus;

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

            DispatcherUnhandledException += (sender, args) =>
            {
                args.Handled = true;
                LogHelper.Current.Log(LogType.Fatal, args.Exception.Message);
                MessageBox.Show(
                    args.Exception.Message,
                    "灾难性故障",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK);
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                LogHelper.Current.Log(LogType.Fatal, (args.ExceptionObject as Exception)?.Message ?? "Exception");
                MessageBox.Show(
                    (args.ExceptionObject as Exception)?.Message ?? "Exception",
                    "灾难性故障",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK);
            };

            // Show Window
            if (MainWindow is null) MainWindow = new MainWindow();
            MainWindow.Show();

            Current.Dispatcher?.Invoke(() => ThemeService.Current.ChangeTheme(Theme.Dark));
            Current.Dispatcher?.Invoke(() => ThemeService.Current.ChangeAccent(Accent.Blue));

            TaskManager.Current.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != nameof(TaskManager.Current.Status)) return;
                if (TaskManager.Current.Status == TaskStatus.Running)
                    Current.Dispatcher?.Invoke(() => ThemeService.Current.ChangeAccent(Accent.Orange));
                else Current.Dispatcher?.Invoke(() => ThemeService.Current.ChangeAccent(Accent.Blue));
            };
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ThemeService.Current.Register(this, Theme.Windows, Accent.Windows);
        }
    }
}
