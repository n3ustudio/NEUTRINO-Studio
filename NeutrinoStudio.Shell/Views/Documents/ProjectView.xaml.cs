using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using NeutrinoStudio.Shell.Models;
using YDock.Interface;

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

        #endregion

        public IDockControl DockControl { get; set; }
        public string Header => "项目";
        public ImageSource Icon => null;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
