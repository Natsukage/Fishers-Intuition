using System;
using System.Windows.Data;

namespace 渔人的直感.Models
{
    /// <summary>
    /// 值转换器，根据进度条的高度设定进度条的圆角半径确保外观统一
    /// </summary>
    [ValueConversion(typeof(double), typeof(double))]
    public class HeightToRadiusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (double)value / 2;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new NotSupportedException();
        }
    }
    /// <summary>
    /// 值转换器，反转boolean类型结果
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
