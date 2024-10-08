﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace DLSwtichBinTextEditor
{
    internal class IntHexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return $"{value:X}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return int.Parse(value as string, NumberStyles.HexNumber);
        }
    }
}
