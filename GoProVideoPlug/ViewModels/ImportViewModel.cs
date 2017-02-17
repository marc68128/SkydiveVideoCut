using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using GoProVideoPlug.Helpers;
using GoProVideoPlug.Models;
using GoProVideoPlug.Properties;
using GoProVideoPlug.Services;
using MediaToolkit;
using MediaToolkit.Options;

namespace GoProVideoPlug.ViewModels
{
    public class ImportViewModel : BaseViewModel
    {
        private string _rootDirectory;
        private VideoService _videoService;
        private DriveService _driveService; 

        public ImportViewModel()
        {
            _rootDirectory = Settings.Default["RootFolderPath"].ToString();
            _videoService = new VideoService("1d7e166bae1e4ea2ad0e0017f9cb44ab");
            _driveService = new DriveService();
            _driveService.DriveAddedEvent += (sender, info) => { SelectedDrive = info.RootDirectory.Name; };              
        }

        private string _selectedDrive;
        public string SelectedDrive
        {
            get { return _selectedDrive; }
            set
            {
                _selectedDrive = value;
                SelectedDriveChanged();
                OnPropertyChanged();
            }
        }


        private void SelectedDriveChanged()
        {

            var videos = IOExtensions.VideoExtensions.SelectMany(f => IOExtensions.GetFilesRecursiv(SelectedDrive, f)).Where(p => !p.StartsWith(".")).Distinct().Select(p => new Video(p));
            var tmpDirectoryPath = Path.Combine(_rootDirectory, "temp");
            if (Directory.Exists(tmpDirectoryPath))
                Directory.Delete(tmpDirectoryPath, true);
            Directory.CreateDirectory(tmpDirectoryPath);
            foreach (var video in videos)
            {
                _videoService.Copy(video, Path.Combine(tmpDirectoryPath)).ContinueWith(async t =>
                {
                    var startTime = await _videoService.GetStartFrame(video);
                    if (startTime.HasValue)
                        await _videoService.Cut(video, startTime.Value - 3, _rootDirectory);
                });                   
            }         
        }
    }
}
