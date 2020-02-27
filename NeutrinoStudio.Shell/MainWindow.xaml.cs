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
using YDock;
using YDock.Enum;
using YDock.Interface;

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

            doc_0 = new Doc("doc_0");
            doc_1 = new Doc("doc_1");
            doc_2 = new Doc("doc_2");
            doc_3 = new Doc("doc_3");
            left = new Doc("left");
            right = new Doc("right");
            top = new Doc("top");
            bottom = new Doc("bottom");
            left_1 = new Doc("left_1");
            right_1 = new Doc("right_1");
            top_1 = new Doc("top_1");
            bottom_1 = new Doc("bottom_1");

            DockManager.RegisterDocument(doc_0);
            DockManager.RegisterDocument(doc_1);
            DockManager.RegisterDocument(doc_2);
            DockManager.RegisterDocument(doc_3);

            DockManager.RegisterDock(left);
            DockManager.RegisterDock(right, DockSide.Right);
            DockManager.RegisterDock(top, DockSide.Top);
            DockManager.RegisterDock(bottom, DockSide.Bottom);

            DockManager.RegisterDock(left_1);
            DockManager.RegisterDock(right_1, DockSide.Right);
            DockManager.RegisterDock(top_1, DockSide.Top);
            DockManager.RegisterDock(bottom_1, DockSide.Bottom);
        }

        private Doc doc_0;
        private Doc doc_1;
        private Doc doc_2;
        private Doc doc_3;
        private Doc left;
        private Doc right;
        private Doc top;
        private Doc bottom;
        private Doc left_1;
        private Doc right_1;
        private Doc top_1;
        private Doc bottom_1;


        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            doc_0.DockControl.Show();
            doc_1.DockControl.Show();
            doc_2.DockControl.Show();
            doc_3.DockControl.Show();
            left.DockControl.Show();
            right.DockControl.Show();
            top.DockControl.Show();
            bottom.DockControl.Show();
            left_1.DockControl.Show();
            right_1.DockControl.Show();
            top_1.DockControl.Show();
            bottom_1.DockControl.Show();
        }
    }

    public class Doc : Grid, IDockSource
    {
        public Doc(string header)
        {
            _header = header;
            Background = new SolidColorBrush(Color.FromArgb(255, 27, 27, 28));
        }

        private IDockControl _dockControl;
        public IDockControl DockControl
        {
            get
            {
                return _dockControl;
            }

            set
            {
                _dockControl = value;
            }
        }

        private string _header;
        public string Header
        {
            get
            {
                return _header;
            }
        }

        public ImageSource Icon
        {
            get
            {
                return null;
            }
        }
    }
}
