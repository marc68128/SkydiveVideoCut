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
        private VisionService _visionService;

        public ImportViewModel()
        {
            _rootDirectory = Settings.Default["RootFolderPath"].ToString();
            _visionService = new VisionService("1d7e166bae1e4ea2ad0e0017f9cb44ab");
            Drives = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Removable).ToList();
            ListenDrive();
        }

        private List<DriveInfo> _drives;
        public List<DriveInfo> Drives
        {
            get { return _drives; }
            set
            {
                _drives = value;
                OnPropertyChanged();
            }
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

        private async void ListenDrive()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    var drives = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Removable).ToList();
                    if (drives.Count > _drives.Count)
                    {
                        var addedDrive = drives.Except(_drives).First();
                        Drives = drives;
                        SelectedDrive = addedDrive.RootDirectory.FullName;
                    }
                    if (drives.Count < _drives.Count)
                    {
                        Drives = drives;
                    }
                }
            });
        }

        private void SelectedDriveChanged()
        {

            var videos = IOExtensions.VideoExtensions.SelectMany(f => IOExtensions.GetFilesRecursiv(SelectedDrive, f)).Where(p => !p.StartsWith(".")).Distinct().Select(p => new Video(p, LoadingService));
            var tmpDirectoryPath = Path.Combine(_rootDirectory, "temp");
            if (Directory.Exists(tmpDirectoryPath))
                Directory.Delete(tmpDirectoryPath, true);
            Directory.CreateDirectory(tmpDirectoryPath);
            foreach (var video in videos)
            {
                video.Copy(Path.Combine(tmpDirectoryPath)).ContinueWith(async t =>
                {
                    var startTime = await _visionService.GetStartFrame(video);
                    if (startTime.HasValue)
                        await video.Cut(startTime.Value - 3, _rootDirectory);
                });             
            }
           




        }

    }
}
