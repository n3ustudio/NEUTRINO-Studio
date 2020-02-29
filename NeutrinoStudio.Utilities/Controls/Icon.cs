using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace NeutrinoStudio.Utilities.Controls
{
    public sealed class Icon: UserControl
    {
        public Icon()
        {
            TypePropertyChangedCallback(this, new DependencyPropertyChangedEventArgs());
        }

        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(
            "Type",
            typeof(string),
            typeof(Icon),
            new PropertyMetadata(
                "Account",
                TypePropertyChangedCallback));

        private static void TypePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                Icon icon = ((Icon) d);
                if (string.IsNullOrEmpty(icon.Type)) return;
                using (FileStream fs =
                    new FileStream(
                        Path.Combine(Environment.CurrentDirectory, $"Assets/Icons/{icon.Type}/{icon.Type}_16x.xaml"),
                        FileMode.Open))
                {
                    icon.CurrentChild = XamlReader.Load(fs) as DependencyObject;
                    icon.Content = icon.CurrentChild;
                }
            }
            catch
            {
                // ignored
            }
        }

        private DependencyObject CurrentChild { get; set; }

        /// <summary>
        /// The type of icon.
        /// </summary>
        public string Type
        {
            get => (string) GetValue(TypeProperty);
            set => SetValue(TypeProperty, value);
        }

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
            "Size", typeof(double), typeof(Icon), new PropertyMetadata((double) 1, SizePropertyChangedCallback));

        private static void SizePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                Icon icon = ((Icon)d);
                icon.SetValue(RenderTransformProperty,
                    new TransformGroup()
                    {
                        Children =
                        {
                            new ScaleTransform(icon.Size, icon.Size, icon.ActualWidth / 2, icon.ActualHeight / 2)
                        }
                    });
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// The size of icon.
        /// </summary>
        public double Size
        {
            get => (double) GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        public new Transform RenderTransform => (Transform)GetValue(RenderTransformProperty);

        public new double ActualHeight => (double) GetValue(ActualWidthProperty);

        #region IconList

        private static List<string> _iconList;

        public static List<string> IconList
        {
            get
            {
                if (!(_iconList is null)) return _iconList;
                _iconList = new List<string>();
                DirectoryInfo info = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "Assets/Icons"));
                DirectoryInfo[] dirs = info.GetDirectories();
                foreach (DirectoryInfo dir in dirs)
                {
                    _iconList.Add(dir.Name);
                }
                return _iconList;
            }
        }

        #endregion
    }
}
