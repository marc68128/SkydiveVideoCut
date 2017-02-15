﻿using System;
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
using GoProVideoPlug.Pages;
using GoProVideoPlug.Properties;

namespace GoProVideoPlug
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            if (string.IsNullOrEmpty((string)Settings.Default["RootFolderPath"]))
                Settings.Default["RootFolderPath"] = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
           
            NavigateToVideos(null, null);
        }

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
    }
}
