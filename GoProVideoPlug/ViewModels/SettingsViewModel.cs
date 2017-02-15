using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoProVideoPlug.Properties;

namespace GoProVideoPlug.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public SettingsViewModel()
        {
            RootFolderPath = Settings.Default["RootFolderPath"].ToString();

            this.PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            Settings.Default["RootFolderPath"] = RootFolderPath;
        }

        private string _rootFolderPath;
        public string RootFolderPath
        {
            get { return _rootFolderPath; }
            set
            {
                _rootFolderPath = value;
                OnPropertyChanged();
            }
        }

    }
}
