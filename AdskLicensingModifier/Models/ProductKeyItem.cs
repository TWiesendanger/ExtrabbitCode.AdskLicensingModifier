namespace AdskLicensingModifier.Models;

public sealed class ProductKeyItem(string product, string key)
{
    public string Product { get; } = product;
    public string Key { get; } = key;
}