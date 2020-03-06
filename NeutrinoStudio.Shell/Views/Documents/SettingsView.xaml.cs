using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
using Microsoft.WindowsAPICodePack.Dialogs;
using NeutrinoStudio.Shell.Helpers;
using YDock.Interface;
using Path = System.IO.Path;

namespace NeutrinoStudio.Shell.Views.Documents
{
    /// <summary>
    /// SettingsView.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsView : UserControl, IDockSource, INotifyPropertyChanged
    {
        public SettingsView()
        {
            InitializeComponent();

            DataContext = this;

            Loaded += OnLoaded;
        }

        public static SettingsView Current = new SettingsView();

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            NeutrinoDir = ConfigHelper.Current.NeutrinoDir ?? "未配置。这将导致生成错误。";
        }

        #region DataContext

        public string NeutrinoDir
        {
            get => _neutrinoDir;
            set
            {
                _neutrinoDir = value;
                OnPropertyChanged(nameof(NeutrinoDir));
            }
        }

        private string _neutrinoDir = "正在读取配置……";

        #endregion

        public IDockControl DockControl { get; set; }
        public string Header => "设置";
        public ImageSource Icon => null;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetNeutrinoDirButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog fileDialog = new CommonOpenFileDialog()
            {
                Title = "选择 NEUTRINO 目录",
                DefaultDirectory = Environment.CurrentDirectory,
                IsFolderPicker = true,
                AllowNonFileSystemItems = true,
                DefaultFileName = "NEUTRINO",
                EnsurePathExists = true
            };
            if (fileDialog.ShowDialog() != CommonFileDialogResult.Ok) return;
            string dir = fileDialog.FileName;
            if (!File.Exists(Path.Combine(dir, "bin/NEUTRINO.exe")))
            {
                MessageBox.Show(
                    "该目录内没有有效的 NEUTRINO 可执行程序。\n请确保所选 NEUTRINO 文件夹中包含 bin 文件夹。",
                    "目录无效",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK);
                return;
            }

            ConfigHelper.Current.NeutrinoDir = dir;
            NeutrinoDir = dir;
        }

        private void OpenModelsButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            string dir = ConfigHelper.Current.NeutrinoDir;
            if ((!string.IsNullOrEmpty(dir)) && Directory.Exists(dir))
                Process.Start(Path.Combine(dir, "model/"));
            else
                MessageBox.Show(
                    "NEUTRINO 目录无效。",
                    "目录无效",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK);
        }
    }
}
