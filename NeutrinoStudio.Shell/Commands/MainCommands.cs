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
    }
}
