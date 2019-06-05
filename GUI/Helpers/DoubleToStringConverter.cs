using System;
using System.Globalization;
using System.Windows.Data;

namespace GUI.Helpers
{
	public class DoubleToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is double num)
				return num.ToString(CultureInfo.InvariantCulture);
			throw new ArgumentException($"value ({value}) should be integer");

		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value != null ? double.Parse(value.ToString()) : 0;
		}
	}
}