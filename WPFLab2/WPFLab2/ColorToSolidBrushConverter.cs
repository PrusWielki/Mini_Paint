using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using Color = System.Windows.Media.Color;

namespace WPFLab2
{
    public class ColorToSolidBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
           
            SolidColorBrush scb = new SolidColorBrush((Color)value);
            //scb.Color.A = 0;
            
           
            
                return scb;
            
          
                
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
        public class ColorToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //Color a = (Color)value;

            //a.B = (byte)(256 - a.B/2+10);
            //a.R = (byte)(256 - a.R/2 + 10);
            //a.G = (byte)(256 - a.G/2 + 10);
            
            //SolidColorBrush scb = new SolidColorBrush(a);

            



            //return scb;
            var c = (Color)value;
            var l = 0.2126 * c.ScR + 0.7152 * c.ScG + 0.0722 * c.ScB;

            return l < 0.5 ? Brushes.White : Brushes.Black;


        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
