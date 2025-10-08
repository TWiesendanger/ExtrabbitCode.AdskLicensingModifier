using CommunityToolkit.Mvvm.ComponentModel;
// ReSharper disable InconsistentNaming

namespace AdskLicensingModifier.ViewModels;

public partial class ProductKeyViewModel : ObservableObject
{
    [ObservableProperty] private string? searchText;
    [ObservableProperty] private Dictionary<string, string>? adskProducts;
    [ObservableProperty] private Dictionary<string, string>? filteredProducts;


    public Task Initialization { get; }

    public ProductKeyViewModel()
    {

        Initialization = InitializeAsync();
    }

    public async Task InitializeAsync()
    {
        AdskProducts = await ReadProductKeysAsync();
        FilteredProducts = AdskProducts;
    }

    partial void OnSearchTextChanged(string? value)
    {
        if (AdskProducts != null)
        {
            FilteredProducts = string.IsNullOrEmpty(value)
                ? AdskProducts
                : AdskProducts.Where(x => x.Key.Contains(value, StringComparison.OrdinalIgnoreCase))
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }

    public async Task<Dictionary<string, string>> ReadProductKeysAsync()
    {
        var uri2015 = new Uri("ms-appx:///Assets/resources/ProductKeys2015.txt");
        var uri2016 = new Uri("ms-appx:///Assets/resources/ProductKeys2016.txt");
        var uri2017 = new Uri("ms-appx:///Assets/resources/ProductKeys2017.txt");
        var uri2018 = new Uri("ms-appx:///Assets/resources/ProductKeys2018.txt");
        var uri2019 = new Uri("ms-appx:///Assets/resources/ProductKeys2019.txt");
        var uri2020 = new Uri("ms-appx:///Assets/resources/ProductKeys2020.txt");
        var uri2021 = new Uri("ms-appx:///Assets/resources/ProductKeys2021.txt");
        var uri2022 = new Uri("ms-appx:///Assets/resources/ProductKeys2022.txt");
        var uri2023 = new Uri("ms-appx:///Assets/resources/ProductKeys2023.txt");
        var uri2024 = new Uri("ms-appx:///Assets/resources/ProductKeys2024.txt");
        var uri2025 = new Uri("ms-appx:///Assets/resources/ProductKeys2025.txt");
        var uri2026 = new Uri("ms-appx:///Assets/resources/ProductKeys2026.txt");

        var productKeys2015 = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri2015);
        var productKeys2016 = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri2016);
        var productKeys2017 = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri2017);
        var productKeys2018 = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri2018);
        var productKeys2019 = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri2019);
        var productKeys2020 = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri2020);
        var productKeys2021 = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri2021);
        var productKeys2022 = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri2022);
        var productKeys2023 = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri2023);
        var productKeys2024 = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri2024);
        var productKeys2025 = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri2025);
        var productKeys2026 = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri2026);

        var productKeyList2015 = await Windows.Storage.FileIO.ReadLinesAsync(productKeys2015);
        var productKeyList2016 = await Windows.Storage.FileIO.ReadLinesAsync(productKeys2016);
        var productKeyList2017 = await Windows.Storage.FileIO.ReadLinesAsync(productKeys2017);
        var productKeyList2018 = await Windows.Storage.FileIO.ReadLinesAsync(productKeys2018);
        var productKeyList2019 = await Windows.Storage.FileIO.ReadLinesAsync(productKeys2019);
        var productKeyList2020 = await Windows.Storage.FileIO.ReadLinesAsync(productKeys2020);
        var productKeyList2021 = await Windows.Storage.FileIO.ReadLinesAsync(productKeys2021);
        var productKeyList2022 = await Windows.Storage.FileIO.ReadLinesAsync(productKeys2022);
        var productKeyList2023 = await Windows.Storage.FileIO.ReadLinesAsync(productKeys2023);
        var productKeyList2024 = await Windows.Storage.FileIO.ReadLinesAsync(productKeys2024);
        var productKeyList2025 = await Windows.Storage.FileIO.ReadLinesAsync(productKeys2025);
        var productKeyList2026 = await Windows.Storage.FileIO.ReadLinesAsync(productKeys2026);

        var combinedList = new List<string>();

        combinedList.AddRange(productKeyList2015);
        combinedList.AddRange(productKeyList2016);
        combinedList.AddRange(productKeyList2017);
        combinedList.AddRange(productKeyList2018);
        combinedList.AddRange(productKeyList2019);
        combinedList.AddRange(productKeyList2020);
        combinedList.AddRange(productKeyList2021);
        combinedList.AddRange(productKeyList2022);
        combinedList.AddRange(productKeyList2023);
        combinedList.AddRange(productKeyList2024);
        combinedList.AddRange(productKeyList2025);
        combinedList.AddRange(productKeyList2026);

        var splitParts = combinedList.Select((line => line.Split(';'))).ToArray();
        var dict = new Dictionary<string, string>();

        foreach (var part in splitParts)
        {
            if (dict.ContainsKey(part[0]))
            {
                continue;
            }

            try
            {
                dict.Add(part[0], part[1]);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        return dict;
    }
}
