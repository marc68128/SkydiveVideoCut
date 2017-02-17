using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoProVideoPlug.Services
{
    public class DriveService
    {
        private List<DriveInfo> _drives;

        public bool IsListening { get; set; }

        public async void StartListening()
        {
            _drives = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Removable).ToList();
            IsListening = true;
            await Task.Run(() =>
            {
                while (IsListening)
                {
                    var drives = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Removable).ToList();
                    if (drives.Count > _drives.Count)
                    {
                        var addedDrive = drives.Except(_drives).First();
                        _drives = drives;
                        DriveAddedEvent?.Invoke(this, addedDrive);
                    }
                    if (drives.Count < _drives.Count)
                    {
                        _drives = drives;
                    }
                }
            });
        }
        public void StopListening()
        {
            IsListening = false; 
        }

        public event EventHandler<DriveInfo> DriveAddedEvent;
    }
}
