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
using NeutrinoStudio.Shell.Commands;
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

            Closed += (sender, args) => Application.Current.Shutdown(0);

            DataContext = this;

            #region Command Bindings

            CommandBindings.Add(new CommandBinding(
                UICommands.ExitApp,
                (o, args) => Application.Current.Shutdown(0),
                (o, args) => args.CanExecute = true));

            #endregion

            _welcomeView = new WelcomeView();
            _logView = new LogView();
            _debugView = new DebugView();
            DockManager.RegisterDocument(_welcomeView);
            DockManager.RegisterDock(_logView, DockSide.Bottom);
            DockManager.RegisterDocument(_debugView);
        }

        #region Views

        private readonly WelcomeView _welcomeView;
        private readonly LogView _logView;
        private readonly DebugView _debugView;

        #endregion

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _logView.DockControl.Show();
            _welcomeView.DockControl.Show();
            _debugView.DockControl.Show();
        }
    }
}
