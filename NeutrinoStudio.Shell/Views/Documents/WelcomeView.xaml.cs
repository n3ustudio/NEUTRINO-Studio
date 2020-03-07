using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Squirrel;
using YDock.Interface;

namespace NeutrinoStudio.Shell.Views.Documents
{
    /// <summary>
    /// WelcomeView.xaml 的交互逻辑
    /// </summary>
    public partial class WelcomeView : UserControl, IDockSource
    {
        public WelcomeView()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        public static WelcomeView Current = new WelcomeView();

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            CurrentVersion.Text = $"当前版本 {Assembly.GetExecutingAssembly().GetName().Version}";
        }

        public IDockControl DockControl { get; set; }
        public string Header => Properties.Resources.WelcomeView_Title;
        public ImageSource Icon => null;

        private void CheckUpdateButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            UpdateCheckButton.IsEnabled = false;
            Task.Factory.StartNew(async () =>
            {
                using (var mgr = new UpdateManager("https://n3ustudio.vbox.moe/res/releases"))
                {
                    await mgr.UpdateApp();
                    Application.Current.Dispatcher?.Invoke(() =>
                        UpdateCheckButton.IsEnabled = true);
                }
            });
        }
    }
}
