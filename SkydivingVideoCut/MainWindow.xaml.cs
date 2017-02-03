using System.Windows;
using SkydivingVideoCut.Pages;

namespace SkydivingVideoCut
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Content = new HomePage();
        }
    }
}
