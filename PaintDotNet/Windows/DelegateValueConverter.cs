namespace PaintDotNet.Windows
{
    using PaintDotNet.Diagnostics;
    using System;
    using System.Globalization;
    using System.Windows.Data;

    internal sealed class DelegateValueConverter : IValueConverter
    {
        private Func<object, object> convertBackFn;
        private Func<object, object> convertFn;

        public DelegateValueConverter(Func<object, object> convertFn) : this(convertFn, null)
        {
        }

        public DelegateValueConverter(Func<object, object> convertFn, Func<object, object> convertBackFn)
        {
            Validate.IsNotNull<Func<object, object>>(convertFn, "convertFn");
            this.convertFn = convertFn;
            this.convertBackFn = convertBackFn;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => 
            this.convertFn(value);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => 
            this.convertBackFn?.Invoke(value);
    }
}

