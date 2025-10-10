using Windows.UI;

namespace AdskLicensingModifier.Views;

public partial class MessageDialog
{
    public string Message { get; }
    public string Symbol { get; set; }
    public Color BackgroundColor { get; }

    public MessageDialog(string message, Color backgroundColor, string symbol)
    {
        InitializeComponent();
        Message = message;
        Symbol = symbol;
        BackgroundColor = backgroundColor;
    }
}