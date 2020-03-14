using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using NeuTask;
using NeutrinoStudio.Core.Tasks;
using NeutrinoStudio.Shell.Helpers;
using NeutrinoStudio.Shell.Models;
using YDock.Interface;
using Path = System.IO.Path;

namespace NeutrinoStudio.Shell.Views.Documents
{
    /// <summary>
    /// ProjectView.xaml 的交互逻辑
    /// </summary>
    public partial class ProjectView : UserControl, IDockSource, INotifyPropertyChanged
    {
        public ProjectView()
        {
            InitializeComponent();

            DataContext = this;
        }

        public static ProjectView Current = new ProjectView();

        #region DataContext

        private InputFormat _inputFormat = InputFormat.Undefined;

        public InputFormat InputFormat
        {
            get => _inputFormat;
            set
            {
                _inputFormat = value;
                OnPropertyChanged(nameof(InputFormat));
            }
        }

        private OutputFormat _outputFormat = OutputFormat.Wav;

        public OutputFormat OutputFormat
        {
            get => _outputFormat;
            set
            {
                _outputFormat = value;
                OnPropertyChanged(nameof(OutputFormat));
            }
        }

        private string _modelDir = "";

        public string ModelDir
        {
            get => _modelDir;
            set
            {
                _modelDir = value;
                OnPropertyChanged(nameof(ModelDir));
            }
        }

        private string _projectName = "";

        public string ProjectName
        {
            get => _projectName;
            set
            {
                _projectName = value;
                OnPropertyChanged(nameof(ProjectName));
            }
        }

        private string _projectDir = "";

        public string ProjectDir
        {
            get => _projectDir;
            set
            {
                _projectDir = value;
                OnPropertyChanged(nameof(ProjectDir));
            }
        }

        private string _inputDir = "";

        public string InputDir
        {
            get => _inputDir;
            set
            {
                _inputDir = value;
                OnPropertyChanged(nameof(InputDir));
            }
        }

        private string _labelDir = "";

        public string LabelDir
        {
            get => _labelDir;
            set
            {
                _labelDir = value;
                OnPropertyChanged(nameof(LabelDir));
            }
        }

        private string _convertDir = "";

        public string ConvertDir
        {
            get => _convertDir;
            set
            {
                _convertDir = value;
                OnPropertyChanged(nameof(ConvertDir));
            }
        }

        private string _generateDir = "";

        private string _synthDir = "";

        public string SynthDir
        {
            get => _synthDir;
            set
            {
                _synthDir = value;
                OnPropertyChanged(nameof(SynthDir));
            }
        }

        private string _outputDir = "";

        public string OutputDir
        {
            get => _outputDir;
            set
            {
                _outputDir = value;
                OnPropertyChanged(nameof(OutputDir));
            }
        }

        private bool _isLabelEnabled = true;

        public bool IsLabelEnabled
        {
            get => _isLabelEnabled;
            set
            {
                _isLabelEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool _isSynthEnabled = true;

        public bool IsSynthEnabled
        {
            get => _isSynthEnabled;
            set
            {
                _isSynthEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool _isOutputEnabled = true;

        public bool IsOutputEnabled
        {
            get => _isOutputEnabled;
            set
            {
                _isOutputEnabled = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public IDockControl DockControl { get; set; }
        public string Header => Properties.Resources.ProjectView_Title;
        public ImageSource Icon => null;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Logic

        private void InputButtonBase_OnClick(object sender, RoutedEventArgs e)
        {

            CommonFileDialogFilter filter;

            switch (InputFormat)
            {
                case InputFormat.MusicXml:
                    filter = new CommonFileDialogFilter("Music XML", ".musicxml");
                    break;
                case InputFormat.Xml:
                    filter = new CommonFileDialogFilter("Music XML", ".xml");
                    break;
                case InputFormat.Vsq:
                    filter = new CommonFileDialogFilter("VOCALOID 3 Project", ".vsq");
                    break;
                case InputFormat.Vsqx:
                    filter = new CommonFileDialogFilter("VOCALOID 4 Project", ".vsqx");
                    break;
                case InputFormat.Vpr:
                    filter = new CommonFileDialogFilter("VOCALOID 5 Project", ".vpr");
                    break;
                case InputFormat.Ust:
                    filter = new CommonFileDialogFilter("UTAU Project", ".ust");
                    break;
                case InputFormat.Ccs:
                    filter = new CommonFileDialogFilter("CeVIO Project", ".ccs");
                    break;
                default:
                    return;
            }

            CommonOpenFileDialog fileDialog = new CommonOpenFileDialog
            {
                Title = "选择输入文件",
                DefaultDirectory = Environment.CurrentDirectory,
                IsFolderPicker = false,
                AllowNonFileSystemItems = true,
                EnsurePathExists = true,
                Multiselect = false,
                Filters = { filter },
                EnsureFileExists = true
            };

            if (fileDialog.ShowDialog() != CommonFileDialogResult.Ok) return;
            InputDir = fileDialog.FileName;

            #region Autofill

            FileInfo file = new FileInfo(InputDir);
            string dir = file.DirectoryName;
            if (string.IsNullOrEmpty(dir)) return;
            ProjectName = file.Name.Replace(file.Extension, "");
            ProjectDir = Path.Combine(dir, ProjectName);
            LabelDir = Path.Combine(ProjectDir, "label");
            SynthDir = Path.Combine(ProjectDir, "output");
            OutputDir = Path.Combine(ProjectDir, "output");

            #endregion

        }

        private void LabelButtonBase_OnClick(object sender, RoutedEventArgs e)
        {

            CommonOpenFileDialog fileDialog = new CommonOpenFileDialog
            {
                Title = "选择转换目录",
                DefaultDirectory = Environment.CurrentDirectory,
                IsFolderPicker = true,
                AllowNonFileSystemItems = true,
                EnsurePathExists = true
            };

            if (fileDialog.ShowDialog() != CommonFileDialogResult.Ok) return;
            LabelDir = fileDialog.FileName;

        }

        private void ModelButtonBase_OnClick(object sender, RoutedEventArgs e)
        {

            CommonOpenFileDialog fileDialog = new CommonOpenFileDialog
            {
                Title = "选择模型目录",
                DefaultDirectory = Environment.CurrentDirectory,
                IsFolderPicker = true,
                AllowNonFileSystemItems = true,
                EnsurePathExists = true
            };

            if (fileDialog.ShowDialog() != CommonFileDialogResult.Ok) return;
            ModelDir = fileDialog.FileName;

        }
        private void SynthButtonBase_OnClick(object sender, RoutedEventArgs e)
        {

            CommonOpenFileDialog fileDialog = new CommonOpenFileDialog
            {
                Title = "选择生成目录",
                DefaultDirectory = Environment.CurrentDirectory,
                IsFolderPicker = true,
                AllowNonFileSystemItems = true,
                EnsurePathExists = true
            };

            if (fileDialog.ShowDialog() != CommonFileDialogResult.Ok) return;
            SynthDir = fileDialog.FileName;

        }

        private void OutputButtonBase_OnClick(object sender, RoutedEventArgs e)
        {

            CommonOpenFileDialog fileDialog = new CommonOpenFileDialog
            {
                Title = "选择合成目录",
                DefaultDirectory = Environment.CurrentDirectory,
                IsFolderPicker = true,
                AllowNonFileSystemItems = true,
                EnsurePathExists = true
            };

            if (fileDialog.ShowDialog() != CommonFileDialogResult.Ok) return;
            OutputDir = fileDialog.FileName;

        }

        private void ConvertButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void StartButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (
                string.IsNullOrEmpty(ProjectName) ||
                string.IsNullOrEmpty(ProjectDir) ||
                InputFormat == InputFormat.Undefined ||
                string.IsNullOrEmpty(InputDir) ||
                string.IsNullOrEmpty(LabelDir) ||
                string.IsNullOrEmpty(ModelDir) ||
                string.IsNullOrEmpty(SynthDir) ||
                string.IsNullOrEmpty(OutputDir)
            )
            {
                MessageBox.Show(
                    "参数不正确。请检查内容是否已填写完整。",
                    "参数不正确",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK);
                return;
            }

            try
            {
                Directory.CreateDirectory(ProjectDir);
                DirectoryInfo labelDir = Directory.CreateDirectory(LabelDir);
                Directory.CreateDirectory(SynthDir);
                Directory.CreateDirectory(OutputDir);
                labelDir.CreateSubdirectory("full");
                labelDir.CreateSubdirectory("mono");
                labelDir.CreateSubdirectory("timing");
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    "获取目录时发生错误。",
                    "错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK);
            }

            string labelOutput = Path.Combine(LabelDir, $"mono\\{ProjectName}.lab");
            string neuInputFull = Path.Combine(LabelDir, $"full\\{ProjectName}.lab");
            string neuInputTiming = Path.Combine(LabelDir, $"timing\\{ProjectName}.lab");
            string neuOutputF0 = Path.Combine(SynthDir, $"{ProjectName}.f0");
            string neuOutputMgc = Path.Combine(SynthDir, $"{ProjectName}.mgc");
            string neuOutputBap = Path.Combine(SynthDir, $"{ProjectName}.bap");
            string synthOutput = Path.Combine(OutputDir, $"{ProjectName}.wav");
            
            LogHelper.Current.Log(LogType.Warn, "合成：启动");
            LogHelper.Current.Log(LogType.Info, "配置：");
            LogHelper.Current.Log(LogType.Info, "musicXMLtoLabel:");
            LogHelper.Current.Log(LogType.Info, $"Input: {InputDir}");
            LogHelper.Current.Log(LogType.Info, $"Output: {labelOutput}");
            LogHelper.Current.Log(LogType.Info, "NEUTRINO:");
            LogHelper.Current.Log(LogType.Info, $"Input: {neuInputFull}");
            LogHelper.Current.Log(LogType.Info, $"Input: {neuInputTiming}");
            LogHelper.Current.Log(LogType.Info, $"Output: {neuOutputF0}");
            LogHelper.Current.Log(LogType.Info, $"Output: {neuOutputMgc}");
            LogHelper.Current.Log(LogType.Info, $"Output: {neuOutputBap}");
            LogHelper.Current.Log(LogType.Info, $"Input: {ModelDir}");
            LogHelper.Current.Log(LogType.Info, "WORLD:");
            LogHelper.Current.Log(LogType.Info, $"Input: {neuOutputF0}");
            LogHelper.Current.Log(LogType.Info, $"Input: {neuOutputMgc}");
            LogHelper.Current.Log(LogType.Info, $"Input: {neuOutputBap}");
            LogHelper.Current.Log(LogType.Info, $"Output: {synthOutput}");
            LogHelper.Current.Log(LogType.Warn, "推送到任务序列。");

            if (IsLabelEnabled) TaskManager.Current.Push(new LabelTask(
                ConfigHelper.Current.NeutrinoDir,
                InputDir,
                neuInputFull,
                labelOutput));
            if (IsSynthEnabled) TaskManager.Current.Push(new SynthTask(
                ConfigHelper.Current.NeutrinoDir,
                neuInputFull,
                neuInputTiming,
                neuOutputF0,
                neuOutputMgc,
                neuOutputBap,
                ModelDir));
            if (IsOutputEnabled) TaskManager.Current.Push(new OutputTask(
                ConfigHelper.Current.NeutrinoDir,
                neuOutputF0,
                neuOutputMgc,
                neuOutputBap,
                synthOutput));

        }

        #endregion

    }
}
