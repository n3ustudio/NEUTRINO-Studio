using System;
using System.Collections.Generic;
using System.Linq;
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
        }

        public IDockControl DockControl { get; set; }
        public string Header => "欢迎";
        public ImageSource Icon => null;

        private void NoProjectButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            
        }

        private void CheckUpdateButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            UpdateCheckButton.IsEnabled = false;
            using (var mgr = new UpdateManager("https://n3ustudio.vbox.moe/res/releases"))
            {
                mgr.UpdateApp().ContinueWith(
                    (task) =>
                        Application.Current.Dispatcher != null && 
                        Application.Current.Dispatcher.Invoke(() =>
                            UpdateCheckButton.IsEnabled = true)).Start();
            }
        }
    }
}
