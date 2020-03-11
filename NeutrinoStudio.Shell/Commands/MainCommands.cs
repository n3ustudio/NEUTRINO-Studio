using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NeutrinoStudio.Shell.Properties;

namespace NeutrinoStudio.Shell.Commands
{
    public static class UICommands
    {

        public static RoutedUICommand ExitApp { get; } = new RoutedUICommand(
            Resources.Menu_Quit,
            "Exit",
            typeof(UICommands),
            new InputGestureCollection(new List<InputGesture>()
            {
                new KeyGesture(Key.F4, ModifierKeys.Alt, "Alt+F4")
            }));

        public static RoutedUICommand OpenWelcomeView { get; } = new RoutedUICommand(
            Resources.Menu_Welcome,
            "Welcome",
            typeof(UICommands));

        public static RoutedUICommand OpenProjectView { get; } = new RoutedUICommand(
            Resources.Menu_Project,
            "Project",
            typeof(UICommands),
            new InputGestureCollection(new List<InputGesture>()
            {
                new KeyGesture(Key.P, ModifierKeys.Control, "Ctrl+P")
            }));

        public static RoutedUICommand OpenSettingsView { get; } = new RoutedUICommand(
            Resources.Menu_Settings,
            "Settings",
            typeof(UICommands),
            new InputGestureCollection(new List<InputGesture>()
            {
                new KeyGesture(Key.W, ModifierKeys.Control, "Ctrl+W")
            }));

        public static RoutedUICommand OpenDebugView { get; } = new RoutedUICommand(
            Resources.Menu_Debug,
            "Debug",
            typeof(UICommands));

        public static RoutedUICommand OpenLogView { get; } = new RoutedUICommand(
            Resources.Menu_Log,
            "Log",
            typeof(UICommands),
            new InputGestureCollection(new List<InputGesture>()
            {
                new KeyGesture(Key.L, ModifierKeys.Control, "Ctrl+L")
            }));

        public static RoutedUICommand OpenTaskView { get; } = new RoutedUICommand(
            Resources.Menu_TaskView,
            "Task",
            typeof(UICommands),
            new InputGestureCollection(new List<InputGesture>()
            {
                new KeyGesture(Key.T, ModifierKeys.Control, "Ctrl+T")
            }));

        public static RoutedUICommand Monitor { get; } = new RoutedUICommand(
            Resources.Menu_Monitor,
            "Monitor",
            typeof(UICommands),
            new InputGestureCollection(new List<InputGesture>()
            {
                new KeyGesture(Key.F6, ModifierKeys.None, "F6")
            }));

        public static RoutedUICommand Generate { get; } = new RoutedUICommand(
            Resources.Menu_Generate,
            "Generate",
            typeof(UICommands),
            new InputGestureCollection(new List<InputGesture>()
            {
                new KeyGesture(Key.F5, ModifierKeys.None, "F5")
            }));

        public static RoutedUICommand Queue { get; } = new RoutedUICommand(
            Resources.Menu_Queue,
            "Queue",
            typeof(UICommands),
            new InputGestureCollection(new List<InputGesture>()
            {
                new KeyGesture(Key.F7, ModifierKeys.None, "F7")
            }));

        public static RoutedUICommand Run { get; } = new RoutedUICommand(
            Resources.Menu_Run,
            "Run",
            typeof(UICommands),
            new InputGestureCollection(new List<InputGesture>()
            {
                new KeyGesture(Key.F8, ModifierKeys.None, "F8")
            }));

        public static RoutedUICommand Stop { get; } = new RoutedUICommand(
            Resources.Menu_Stop,
            "Stop",
            typeof(UICommands),
            new InputGestureCollection(new List<InputGesture>()
            {
                new KeyGesture(Key.F9, ModifierKeys.None, "F9")
            }));

        public static RoutedUICommand OpenNavigatorView { get; } = new RoutedUICommand(
            "Navigator",
            "Navigator",
            typeof(UICommands),
            new InputGestureCollection(new List<InputGesture>()
            {
                new KeyGesture(Key.N, ModifierKeys.Control, "Ctrl+N")
            }));
    }
}
