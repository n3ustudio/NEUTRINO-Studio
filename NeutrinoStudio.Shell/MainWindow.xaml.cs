using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using System.Xml.Linq;
using NeutrinoStudio.Shell.Commands;
using NeutrinoStudio.Shell.Views.Docks;
using NeutrinoStudio.Shell.Views.Documents;
using YDock;
using YDock.Enum;
using YDock.Interface;
using Path = System.IO.Path;

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

            Closing += OnClosing;

            Closed += (sender, args) => Application.Current.Shutdown(0);

            #region Command Bindings

            CommandBindings.Add(new CommandBinding(
                UICommands.ExitApp,
                (o, args) => Application.Current.Shutdown(0),
                (o, args) => args.CanExecute = true));

            CommandBindings.Add(new CommandBinding(
                UICommands.OpenWelcomeWindow,
                (sender, args) => _welcomeView.DockControl.Show(),
                (sender, args) => args.CanExecute = true));

            #endregion

            #region Document Register

            _welcomeView = new WelcomeView();
            _logView = new LogView();
            _debugView = new DebugView();
            _settingsView = new SettingsView();
            DockManager.RegisterDocument(_welcomeView);
            DockManager.RegisterDock(_logView, DockSide.Bottom);
            DockManager.RegisterDocument(_debugView);
            DockManager.RegisterDocument(_settingsView);

            #endregion
        }

        private static readonly string SettingFileName = Path.Combine(Environment.CurrentDirectory, "settings/layout.xml");

        private void OnClosing(object sender, CancelEventArgs e)
        {
            Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "settings"));
            DockManager.SaveCurrentLayout("MainWindow");
            var doc = new XDocument();
            var rootNode = new XElement("Layouts");
            foreach (var layout in DockManager.Layouts.Values)
                layout.Save(rootNode);
            doc.Add(rootNode);
            doc.Save(SettingFileName);
            DockManager.Dispose();
        }

        #region Views

        private readonly WelcomeView _welcomeView;
        private readonly LogView _logView;
        private readonly DebugView _debugView;
        private readonly SettingsView _settingsView;

        #endregion

        private void OnLoaded(object sender, RoutedEventArgs e)
        {

            if (File.Exists(SettingFileName))
            {
                XDocument layout = XDocument.Parse(File.ReadAllText(SettingFileName));
                if (layout.Root != null)
                    foreach (XElement item in layout.Root.Elements())
                    {
                        string name = item.Attribute("Name")?.Value;
                        if (string.IsNullOrEmpty(name)) continue;
                        if (DockManager.Layouts.ContainsKey(name))
                            DockManager.Layouts[name].Load(item);
                        else DockManager.Layouts[name] = new YDock.LayoutSetting.LayoutSetting(name, item);
                    }
                DockManager.ApplyLayout("MainWindow");
            }
            else
            {
                _logView.DockControl.Show();
                _welcomeView.DockControl.Show();
                _debugView.DockControl.Show();
                _settingsView.DockControl.Show();
            }
        }
    }
}
