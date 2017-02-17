using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using GoProVideoPlug.IServices;
using GoProVideoPlug.Properties;
using GoProVideoPlug.Services;
using Microsoft.Practices.Unity;

namespace GoProVideoPlug
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const int DetectionPrecision = 10;
    }
}
