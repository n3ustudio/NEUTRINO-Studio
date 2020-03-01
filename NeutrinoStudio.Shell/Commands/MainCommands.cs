using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeutrinoStudio.Shell.Commands
{
    public static class UICommands
    {

        public static RoutedUICommand ExitApp { get; } = new RoutedUICommand(
            "退出(_Q)",
            "Exit",
            typeof(UICommands),
            new InputGestureCollection(new List<InputGesture>()
            {
                new KeyGesture(Key.F4, ModifierKeys.Alt, "Alt+F4")
            }));

        public static RoutedUICommand OpenWelcomeView { get; } = new RoutedUICommand(
            "欢迎(_W)",
            "Welcome",
            typeof(UICommands));

        public static RoutedUICommand OpenProjectView { get; } = new RoutedUICommand(
            "项目(_P)",
            "Project",
            typeof(UICommands),
            new InputGestureCollection(new List<InputGesture>()
            {
                new KeyGesture(Key.P, ModifierKeys.Control, "Ctrl+P")
            }));

        public static RoutedUICommand OpenSettingsView { get; } = new RoutedUICommand(
            "设置(_S)",
            "Settings",
            typeof(UICommands),
            new InputGestureCollection(new List<InputGesture>()
            {
                new KeyGesture(Key.W, ModifierKeys.Control, "Ctrl+W")
            }));

        public static RoutedUICommand OpenDebugView { get; } = new RoutedUICommand(
            "调试(_D)",
            "Debug",
            typeof(UICommands));

        public static RoutedUICommand OpenLogView { get; } = new RoutedUICommand(
            "日志(_L)",
            "Log",
            typeof(UICommands),
            new InputGestureCollection(new List<InputGesture>()
            {
                new KeyGesture(Key.L, ModifierKeys.Control, "Ctrl+L")
            }));

        public static RoutedUICommand OpenTaskView { get; } = new RoutedUICommand(
            "任务(_T)",
            "Task",
            typeof(UICommands),
            new InputGestureCollection(new List<InputGesture>()
            {
                new KeyGesture(Key.T, ModifierKeys.Control, "Ctrl+T")
            }));

        public static RoutedUICommand Monitor { get; } = new RoutedUICommand(
            "监视(_M)",
            "Monitor",
            typeof(UICommands),
            new InputGestureCollection(new List<InputGesture>()
            {
                new KeyGesture(Key.F6, ModifierKeys.None, "F6")
            }));

        public static RoutedUICommand Generate { get; } = new RoutedUICommand(
            "生成(_G)",
            "Generate",
            typeof(UICommands),
            new InputGestureCollection(new List<InputGesture>()
            {
                new KeyGesture(Key.F5, ModifierKeys.None, "F5")
            }));

        public static RoutedUICommand Queue { get; } = new RoutedUICommand(
            "队列(_Q)",
            "Queue",
            typeof(UICommands),
            new InputGestureCollection(new List<InputGesture>()
            {
                new KeyGesture(Key.F7, ModifierKeys.None, "F7")
            }));

        public static RoutedUICommand Run { get; } = new RoutedUICommand(
            "启动(_R)",
            "Run",
            typeof(UICommands),
            new InputGestureCollection(new List<InputGesture>()
            {
                new KeyGesture(Key.F8, ModifierKeys.None, "F8")
            }));

        public static RoutedUICommand Stop { get; } = new RoutedUICommand(
            "停止(_S)",
            "Stop",
            typeof(UICommands),
            new InputGestureCollection(new List<InputGesture>()
            {
                new KeyGesture(Key.F9, ModifierKeys.None, "F9")
            }));
    }
}
