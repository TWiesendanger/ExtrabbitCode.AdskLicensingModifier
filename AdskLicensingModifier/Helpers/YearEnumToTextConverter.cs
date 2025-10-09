using AdskLicensingModifier.Models;
using Microsoft.UI.Xaml.Data;

namespace AdskLicensingModifier.Helpers;

public sealed partial class YearEnumToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => value is AdskYearEnum y ? ((int)y).ToString() : "";
    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}