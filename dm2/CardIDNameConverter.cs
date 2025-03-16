using System.Globalization;
using System.Windows.Data;

namespace dm2
{
	class CardIDNameConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var id = (uint)value;
			var name = Info.Instance().Search(Info.Instance().Card, id)?.Name;
			if (name == null) name = $"ID : {id}";
			return name;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
