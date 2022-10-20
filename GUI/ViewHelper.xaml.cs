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

namespace AISIN_WFA.GUI
{
    /// <summary>
    /// Interaction logic for ViewHelper.xaml
    /// </summary>
    public partial class ViewHelper : UserControl
    {
        public ViewHelper()
        {
            InitializeComponent();
        }
    }
    public class TrueRedLampOnConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var result = new object();
            if (value == null)
            {
                result = false;
            }
            else
            {
                result = (bool)value ? Brushes.Lime : Brushes.Maroon;
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
