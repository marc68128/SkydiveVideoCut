using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GoProVideoPlug.Helpers;
using GoProVideoPlug.Models;
using GoProVideoPlug.Properties;

namespace GoProVideoPlug.ViewModels
{
    public class ImportViewModel : BaseViewModel
    {
        private string _rootDirectory;

        public ImportViewModel()
        {
            _rootDirectory = Settings.Default["RootFolderPath"].ToString();
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

            var videos = IOExtensions.VideoExtensions.SelectMany(f => IOExtensions.GetFilesRecursiv(SelectedDrive, f)).Where(p => !p.StartsWith(".")).Distinct().Select(p => new Video(p));
            var tmpDirectoryPath = Path.Combine(_rootDirectory, "tmp");
            if (Directory.Exists(tmpDirectoryPath))
                Directory.Delete(tmpDirectoryPath, true);
            Directory.CreateDirectory(tmpDirectoryPath);
            List<Task> copyTasks = new List<Task>();
            foreach (var video in videos)
            {
                copyTasks.Add(video.Copy(Path.Combine(tmpDirectoryPath)));
            }
        }

    }
}
