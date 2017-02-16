using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoProVideoPlug.IServices;

namespace GoProVideoPlug.Services
{
    public class LoadingService : ILoadingService
    {
        private string _currentStatus;
        private bool _isLoading;

        protected List<string> LoadingStatus { get; set; }

        public LoadingService()
        {
            LoadingStatus = new List<string>();
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                IsLoadingChanged?.Invoke(this, value);
            }
        }
        public string CurrentStatus
        {
            get { return _currentStatus; }
            set
            {
                _currentStatus = value;
                CurrentStatusChanged?.Invoke(this, value);
            }
        }

        public async void RemoveLoadingStatus(string status)
        {
            if (!LoadingStatus.Contains(status))
            {
                Debug.WriteLine("[Warning] You tried to remove a loading status who doesn't exist");
                return;
            }
            LoadingStatus.Remove(status);
            TaskFactory factory = new TaskFactory();
            await factory.StartNew(() =>
            {
                if (CurrentStatus == status && LoadingStatus.Any())
                {
                    CurrentStatus = LoadingStatus.First();
                }
                else if (CurrentStatus == status && LoadingStatus.Count == 0)
                {
                    Debug.WriteLine("[LoadingService] Loading End");
                    CurrentStatus = null;
                    IsLoading = false;
                }
            });
        }
        public async void AddLoadingStatus(string status)
        {
            LoadingStatus.Add(status);

            TaskFactory factory = new TaskFactory();
            await factory.StartNew(() =>
            {
                if (!IsLoading)
                {
                    Debug.WriteLine("[LoadingService] Loading Start");
                    IsLoading = true;
                    CurrentStatus = status;
                }
            });
        }

        public event EventHandler<string> CurrentStatusChanged;
        public event EventHandler<bool> IsLoadingChanged;
    }
}
