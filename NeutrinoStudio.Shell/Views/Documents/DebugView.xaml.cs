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
using NeutrinoStudio.Shell.Helpers;
using YDock.Interface;

namespace NeutrinoStudio.Shell.Views.Documents
{
    /// <summary>
    /// DebugView.xaml 的交互逻辑
    /// </summary>
    public partial class DebugView : UserControl, IDockSource
    {
        public DebugView()
        {
            InitializeComponent();
            DataContext = this;
        }

        public static DebugView Current = new DebugView();

        #region DataContext

        public List<string> iconList => Utilities.Controls.Icon.IconList;

        #endregion

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            LogHelper.Current.Log(LogType.Debug, "Log Test");
        }

        public IDockControl DockControl { get; set; }
        public string Header => "调试面板";
        public ImageSource Icon => null;

        private void IconSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView box = sender as ListView;
            TestIcon.Type = box?.SelectedItem as string;
        }
    }
}
