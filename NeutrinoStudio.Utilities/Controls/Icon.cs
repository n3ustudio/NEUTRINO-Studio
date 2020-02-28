using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

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
                default(string),
                TypePropertyChangedCallback));

        private static void TypePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Icon icon = ((Icon) d);
            if (string.IsNullOrEmpty(icon.Type)) return;
            if (!(icon._currentChild is null)) icon.RemoveLogicalChild(icon._currentChild);
            using (FileStream fs = new FileStream(Path.Combine(Environment.CurrentDirectory, $"Assets/Icons/{icon.Type}/{icon.Type}_16x.xaml"), FileMode.Open))
                icon.AddChild((DependencyObject)XamlReader.Load(fs));
        }

        private readonly object _currentChild = null;

        public string Type
        {
            get => (string) GetValue(TypeProperty);
            set => SetValue(TypeProperty, value);
        }
    }
}
