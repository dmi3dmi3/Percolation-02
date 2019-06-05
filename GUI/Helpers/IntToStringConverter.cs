using System;
using System.Globalization;
using System.Windows.Data;

namespace GUI.Helpers
{
	public class IntToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is int num)
				return num.ToString();
			throw new ArgumentException($"value ({value}) should be integer");
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value != null && !string.IsNullOrWhiteSpace(value.ToString()) ? int.Parse(value.ToString()) : 0;
		}
	}
}