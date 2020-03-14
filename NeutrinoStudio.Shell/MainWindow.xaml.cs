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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using Musiqual.Parameter.Views;
using Musiqual.Playback;
using NeutrinoStudio.Shell.Commands;
using NeutrinoStudio.Shell.Helpers;
using NeutrinoStudio.Shell.ViewModels;
using NeutrinoStudio.Shell.Views.Docks;
using NeutrinoStudio.Shell.Views.Documents;
using World.UI.Views;
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
                UICommands.OpenWelcomeView,
                (sender, args) => WelcomeView.Current.DockControl.Show(),
                (sender, args) => args.CanExecute = true));

            CommandBindings.Add(new CommandBinding(
                UICommands.OpenProjectView,
                (sender, args) => ProjectView.Current.DockControl.Show(),
                (sender, args) => args.CanExecute = true));

            CommandBindings.Add(new CommandBinding(
                UICommands.OpenSettingsView,
                (sender, args) => SettingsView.Current.DockControl.Show(),
                (sender, args) => args.CanExecute = true));

            CommandBindings.Add(new CommandBinding(
                UICommands.OpenDebugView,
                (sender, args) => DebugView.Current.DockControl.Show(),
                (sender, args) => args.CanExecute = true));

            CommandBindings.Add(new CommandBinding(
                UICommands.OpenLogView,
                (sender, args) => LogView.Current.DockControl.Show(),
                (sender, args) => args.CanExecute = true));

            CommandBindings.Add(new CommandBinding(
                UICommands.OpenTaskView,
                (sender, args) => TaskView.Current.DockControl.Show(),
                (sender, args) => args.CanExecute = true));

            CommandBindings.Add(new CommandBinding(
                UICommands.OpenNavigatorView,
                (sender, args) => NavigatorView.Current.DockControl.Show(),
                (sender, args) => args.CanExecute = true));

            CommandBindings.Add(new CommandBinding(
                UICommands.OpenWorldView,
                (sender, args) => _worldView.DockControl.Show(),
                (sender, args) => args.CanExecute = true));

            CommandBindings.Add(new CommandBinding(
                UICommands.OpenEditModeView,
                (sender, args) => _editModeView.DockControl.Show(),
                (sender, args) => args.CanExecute = true));

            CommandBindings.Add(new CommandBinding(
                UICommands.OpenPlaybackView,
                (sender, args) => PlaybackView.Current.DockControl.Show(),
                (sender, args) => args.CanExecute = true));

            #endregion

            #region Document Register

            DockManager.RegisterDocument(WelcomeView.Current);
            DockManager.RegisterDock(LogView.Current, DockSide.Bottom);
            DockManager.RegisterDocument(DebugView.Current);
            DockManager.RegisterDocument(SettingsView.Current);
            DockManager.RegisterDocument(ProjectView.Current);
            DockManager.RegisterDock(TaskView.Current, DockSide.Right);
            DockManager.RegisterDock(NavigatorView.Current, DockSide.Bottom);
            DockManager.RegisterDock(PlaybackView.Current, DockSide.Top);

            _worldView = new WorldView(DockManager, NavigatorView.Current.DockControl, scross => Navigator.Current.Scross = scross,
                Navigator.Current.EditMode);
            DockManager.RegisterDock(_worldView, DockSide.Top);
            _editModeView = new EditModeView(Navigator.Current.EditMode);
            DockManager.RegisterDock(_editModeView, DockSide.Top);

            #endregion
        }

        #region Views

        private WorldView _worldView;
        private EditModeView _editModeView;

        #endregion

        private static readonly string SettingFileName = Path.Combine(ConfigHelper.UserDataFolder, "layout.xml");

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

            ConfigHelper.SaveConfig();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(hwnd).AddHook(new HwndSourceHook(WndProc));
            wndList = new List<FrameworkElement>() { Wnd1, Wnd2, Wnd3 };

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
                WelcomeView.Current.DockControl.Show();
            }
        }

        #region CaptionBar Hook

        private List<FrameworkElement> wndList;

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_NCHITTEST)
            {
                if (wndList is null) return IntPtr.Zero;
                Point p = new Point();
                int pInt = lParam.ToInt32();
                p.X = (pInt << 16) >> 16;
                p.Y = pInt >> 16;
                if (WndCaption.PointFromScreen(p).Y > WndCaption.ActualHeight) return IntPtr.Zero;
                foreach (FrameworkElement element in wndList)
                {
                    Point rel = element.PointFromScreen(p);
                    if (rel.X >= 0 && rel.X <= element.ActualWidth && rel.Y >= 0 && rel.Y <= element.ActualHeight)
                    {
                        return IntPtr.Zero;
                    }
                }
                handled = true;
                return new IntPtr(2);
            }

            return IntPtr.Zero;
        }

        private const int WM_NCHITTEST = 0x0084;

        #endregion
    }
}
