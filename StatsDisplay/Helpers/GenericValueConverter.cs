using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace StatsDisplay.Helpers
{
	public abstract class GenericValueConverter<V, T, P> : MarkupExtension, IValueConverter
	{
		public P Parameter { get; set; }

		public GenericValueConverter()
		{

		}
		public GenericValueConverter(P parameter)
		{
			Parameter = parameter;
		}
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return this;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			//if (value.GetType() != typeof(V)) throw new ArgumentException(GetType().Name + ".Convert: value type not " + typeof(V).Name);
			//if (targetType != typeof(T)) throw new ArgumentException(GetType().Name + ".Convert: target type not " + typeof(T).Name);
			if (parameter != null) throw new ArgumentException(GetType().Name + ".Convert: binding contains unexpected parameter");
			return Convert((V)value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			//if (value.GetType() != typeof(T)) throw new ArgumentException(GetType().Name + ".ConvertBack: value type not " + typeof(T).Name);
			//if (targetType != typeof(V)) throw new ArgumentException(GetType().Name + ".ConvertBack: target type not " + typeof(V).Name);
			if (parameter != null) throw new ArgumentException(GetType().Name + ".Convert: binding contains unexpected parameter");
			return ConvertBack((T)value);
		}

		protected virtual T Convert(V value)
		{
			throw new NotImplementedException(GetType().Name + "Convert not implemented");
		}
		protected virtual V ConvertBack(T value)
		{
			throw new NotImplementedException(GetType().Name + "ConvertBack not implemented");
		}
	}

	public abstract class GenericValueConverter<V, T> : GenericValueConverter<V, T, string>
	{
		public GenericValueConverter()
		{

		}
		public GenericValueConverter(string parameter)
			: base(parameter)
		{

		}
	}
}
