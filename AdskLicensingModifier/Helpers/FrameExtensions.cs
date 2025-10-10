namespace AdskLicensingModifier.Helpers;

public static class FrameExtensions
{
    public static object? GetPageViewModel(this Frame frame) =>
        (frame?.Content as FrameworkElement)?.DataContext;
}