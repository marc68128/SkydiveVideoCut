using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GoProVideoPlug.Helpers;
using GoProVideoPlug.IServices;
using GoProVideoPlug.Pages;
using GoProVideoPlug.Properties;
using GoProVideoPlug.Services;
using Microsoft.Practices.Unity;

namespace GoProVideoPlug
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private ILoadingService _loadingService;
        public MainWindow()
        {
            InitializeComponent();

            _loadingService = new LoadingService();
            DependencyInjectionUtil.RegisterInstance<ILoadingService>(_loadingService);

            _loadingService.CurrentStatusChanged += (sender, s) => { Dispatcher.Invoke(() => LoadingTextBlock.Text = s); };
            _loadingService.IsLoadingChanged += (sender, b) =>
            {
                Dispatcher.Invoke(() =>
                {
                    LoadingTextBlock.Visibility = b ? Visibility.Visible : Visibility.Hidden;
                    Loader.Visibility = b ? Visibility.Visible : Visibility.Hidden;
                });
            };

            if (string.IsNullOrEmpty((string)Settings.Default["RootFolderPath"]))
                Settings.Default["RootFolderPath"] = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);

            NavigateToVideos(null, null);
        }

        #region Navigation methods

        private void NavigateToVideos(object sender, MouseButtonEventArgs e)
        {
            PageTitle.Text = "Videos";
            VideoIcon.Opacity = 1;
            SettingsIcon.Opacity = SdIcon.Opacity = 0.2;
            NavigationFrame.Navigate(new VideosPage());
        }

        private void NavigateToSdCard(object sender, MouseButtonEventArgs e)
        {
            PageTitle.Text = "Import";
            SdIcon.Opacity = 1;
            SettingsIcon.Opacity = VideoIcon.Opacity = 0.2;
            NavigationFrame.Navigate(new ImportPage());
        }

        private void NavigateToSettings(object sender, MouseButtonEventArgs e)
        {
            PageTitle.Text = "Réglages";
            SettingsIcon.Opacity = 1;
            VideoIcon.Opacity = SdIcon.Opacity = 0.2;
            NavigationFrame.Navigate(new SettingsPage());
        }

        #endregion
    }
}
