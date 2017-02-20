using Analyser2.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace Analyser2
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var path = @"D:\Marc";
            var files = Directory.EnumerateFiles(path);
            foreach (var filePath in files)
            {
                var video = JsonConvert.DeserializeObject<Video>(File.ReadAllText(filePath));
                File.Open(System.IO.Path.Combine(path, "tmp.csv"), FileMode.Create); 
            }
        }
    }
}
