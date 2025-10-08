using Windows.UI;

namespace AdskLicensingModifier.Helpers;

public static class ResourceHelper
{
    public static Color GetColor(string value)
    {
        var resourceDictionary = new ResourceDictionary
        {
            Source = new Uri("ms-appx:///Styles/Colors.xaml")
        };

        if (resourceDictionary.TryGetValue(value, out var resource))
        {
            return (Color)resource;
        }
        return Color.FromArgb(255, 160, 209, 77);
    }

    public static SolidColorBrush GetBrush(string value)
    {
        var resourceDictionary = new ResourceDictionary
        {
            Source = new Uri("ms-appx:///Styles/Colors.xaml")
        };

        if (resourceDictionary.TryGetValue(value, out var resource))
        {
            return new SolidColorBrush((Color)resource);
        }
        return new SolidColorBrush(Color.FromArgb(255, 160, 209, 77));
    }

    public static string GetString(string value)
    {
        var resourceDictionary = new ResourceDictionary
        {
            Source = new Uri("ms-appx:///Styles/Strings.xaml")
        };

        if (resourceDictionary.TryGetValue(value, out var resource))
        {
            return (string)resource;
        }
        return "";
    }
}