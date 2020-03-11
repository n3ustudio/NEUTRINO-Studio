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
using YDock.Interface;

namespace NeutrinoStudio.Shell.Views.Docks
{
    /// <summary>
    /// NavigatorView.xaml 的交互逻辑
    /// </summary>
    public partial class NavigatorView : UserControl, IDockSource
    {
        public NavigatorView()
        {
            InitializeComponent();
        }

        #region Dock

        public IDockControl DockControl { get; set; }

        public string Header => "Navigate";

        public ImageSource Icon => null;

        #endregion

    }
}
