using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using NeutrinoStudio.Shell.Views.Docks;
using NeutrinoStudio.Shell.Views.Documents;
using YDock;
using YDock.Enum;
using YDock.Interface;

namespace NeutrinoStudio.Shell
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += OnLoaded;

            welcomeView = new WelcomeView();
            logView = new LogView();
            DockManager.RegisterDocument(welcomeView);
            DockManager.RegisterDock(logView, DockSide.Bottom);
        }

        #region Views

        private WelcomeView welcomeView;
        private LogView logView;

        #endregion

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            welcomeView.DockControl.Show();
            logView.DockControl.Show();
        }
    }
}
