using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;
using SkydivingVideoCut.Helpers;
using SkydivingVideoCut.Models;
using SkydivingVideoCut.Services;

namespace SkydivingVideoCut.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        private readonly VisionService _visionService;
        private string _videoPath;
        private Video _video;
        private long? _startFrame;
        private double? _startTime;
        private bool _startFound;

        public HomeViewModel()
        {
            _visionService = new VisionService("1d7e166bae1e4ea2ad0e0017f9cb44ab");
            InitCommands();
        }

        public string VideoPath
        {
            get { return _videoPath; }
            set
            {
                _videoPath = value;
                OnPropertyChanged();
            }
        }
        public long? StartFrame
        {
            get { return _startFrame; }
            set
            {
                _startFrame = value;
                OnPropertyChanged();
            }
        }
        public double? StartTime
        {
            get { return _startTime; }
            set
            {
                _startTime = value;
                OnPropertyChanged();
            }
        }
        public bool StartFound
        {
            get { return _startFound; }
            set
            {
                _startFound = value;
                OnPropertyChanged();
            }
        }

        public ActionCommand SelectVideoCommand { get; private set; }
        public ActionCommand AnalizeCommand { get; private set; }

        private void InitCommands()
        {
            SelectVideoCommand = new ActionCommand(() =>
            {
                OpenFileDialog dialog = new OpenFileDialog
                {
                    Multiselect = false,
                    DefaultExt = ".mp4",
                    Filter = "MP4 Files (*.mp4)|*.mp4"
                };

                if (dialog.ShowDialog() != true) return;

                VideoPath = dialog.FileName;
                _video = new Video(dialog.FileName);
                AnalizeCommand.IsEnabled = true; 
            });

            AnalizeCommand = new ActionCommand(async () =>
            {
                IsLoading = true;
                StartFrame = await _visionService.GetStartFrame(_video);
                StartTime = StartFrame / (double)_video.FrameRate;
                StartFound = StartTime.HasValue;
                IsLoading = false; 
            }, false);
        }
    }
}
