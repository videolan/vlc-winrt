using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.System.Threading;
using VLC_WinRT.Utils;
using Windows.UI.Core;

namespace VLC_WinRT.ViewModels.RemovableDevicesVM
{
    public class SpecialThanksViewModel : BindableBase
    {
        private ObservableCollection<BackerCountryViewModel> _backerCountries = new ObservableCollection<BackerCountryViewModel>();

        public SpecialThanksViewModel()
        {
            var _ = ThreadPool.RunAsync(async aa =>
            {
                List<Backer> backers = await ParseBackers();
                var backerDictionary = new SortedDictionary<string, List<string>>();

                foreach (Backer backer in backers)
                {
                    if (!backerDictionary.ContainsKey(backer.Country))
                    {
                        backerDictionary.Add(backer.Country, new List<string>());
                    }
                    backerDictionary[backer.Country].Add(backer.Name);
                }

                var backerCountries = new ObservableCollection<BackerCountryViewModel>();
                foreach (string countryName in backerDictionary.Keys)
                {
                    if (!string.IsNullOrWhiteSpace(countryName))
                    {
                        string flagPath = "ms-appx:///Assets/Flags/flag_of_" + countryName.Replace(" ", "_") + ".png";
                        var backerCountry = new BackerCountryViewModel(countryName, new Uri(flagPath));
                        backerCountry.BackerNames = new List<string>(backerDictionary[countryName]);
                        backerCountries.Add(backerCountry);
                    }
                }
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () => BackerCountries = backerCountries);
            });
        }

        public ObservableCollection<BackerCountryViewModel> BackerCountries
        {
            get { return _backerCountries; }
            private set { SetProperty(ref _backerCountries, value); }
        }

        public async Task<List<Backer>> ParseBackers()
        {
            var backers = new List<Backer>();
            string path = @"backers.csv";
            StorageFolder folder = Package.Current.InstalledLocation;
            StorageFile file = await folder.GetFileAsync(path);
            string csv = await FileIO.ReadTextAsync(file);
            csv = csv.Remove(0, 12);
            string[] lines = csv.Split('\n');

            foreach (string line in lines)
            {
                if (line == "")
                {
                    continue;
                }
                string[] item = line.Split(';');

                var backer = new Backer {Name = item[0].Trim(), Country = item[1].Trim()};
                backers.Add(backer);
            }

            return backers;
        }
    }

    public class Backer
    {
        public string Name { get; set; }
        public string Country { get; set; }
    }
}