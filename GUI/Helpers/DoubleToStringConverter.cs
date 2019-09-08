using System;
using System.Globalization;
using System.Windows.Data;

namespace GUI.Helpers
{
	public class DoubleToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			switch (value)
			{
				case double num:
					return num.ToString(CultureInfo.InvariantCulture);
				case decimal dec:
					return dec.ToString(CultureInfo.InvariantCulture);
				default:
					throw new ArgumentException($"value ({value}) should be double");
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null || !string.IsNullOrWhiteSpace((string) value))
				return 0;
			if (double.TryParse(value.ToString(), out var res))
				return res;
			return decimal.Parse(value.ToString());
		}
	}
}