using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Video.FFMPEG;
using GoProVideoPlug.IServices;
using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;

namespace GoProVideoPlug.Models
{
    public class Video
    {
        private string _path;
        private ILoadingService _loadingService;
        private readonly VideoFileReader _reader = new VideoFileReader();
        public int FrameRate { get; set; }
        public long FrameCount { get; set; }
        public long Duration { get; set; }
        public long CurrentFrame { get; private set; }
        public DateTime CreationDate { get; set; }

        public Video(string path, ILoadingService loadingService)
        {
            _path = path;
            _loadingService = loadingService;
            CreationDate = File.GetCreationTime(_path);
            _reader.Open(path);
            FrameCount = _reader.FrameCount;
            FrameRate = _reader.FrameRate;
            Duration = FrameCount / FrameRate;
        }

        public Task SkipFrame(int count = 1)
        {
            return Task.Run(() =>
            {
                for (int i = 0; i < count; i++)
                {
                    _reader.ReadVideoFrame().Dispose();
                    CurrentFrame++;
                }
            });
        }
        public Task<Bitmap> GetFrame()
        {
            if (CurrentFrame > FrameCount)
                throw new ArgumentOutOfRangeException($"No more frame in this video");

            return Task.Run(() =>
            {
                CurrentFrame++;
                return _reader.ReadVideoFrame();
            });
        }
        public Task<Bitmap> GetFrame(long frame)
        {
            if (!_reader.IsOpen)
            {
                _reader.Open(_path);
                CurrentFrame = 0;
            }

            if (frame < CurrentFrame)
            {
                _reader.Close();
                _reader.Open(_path);
                CurrentFrame = 0;
            }
            return Task.Run(() =>
            {
                while (CurrentFrame < frame)
                {
                    _reader.ReadVideoFrame().Dispose();
                    CurrentFrame++;
                }
                CurrentFrame = frame;
                return _reader.ReadVideoFrame();
            });
        }

        public Task Copy(string newDirectory)
        {
            return Task.Run(() =>
            {
                _loadingService.AddLoadingStatus($"Copie de {Path.GetFileNameWithoutExtension(_path)} vers le dossier temporaire...");
                var newPath = Path.Combine(newDirectory, Path.GetFileName(_path));
                File.Copy(_path, newPath);
                _path = newPath;
                _reader.Close();
                _reader.Open(_path);
                CurrentFrame = 0;
                _loadingService.RemoveLoadingStatus($"Copie de {Path.GetFileNameWithoutExtension(_path)} vers le dossier temporaire...");
            });
        }

        public Task Cut(long jumpStartTime, string outRootPath)
        {
            return Task.Run(() =>
            {
                var inputFile = new MediaFile { Filename = _path };

                var outFolder = Path.Combine(outRootPath, CreationDate.Year.ToString(), CreationDate.ToString("MMMM"), CreationDate.Day.ToString());

                if (!Directory.Exists(outFolder))
                    Directory.CreateDirectory(outFolder);

                var planeOutputFile = new MediaFile { Filename = Path.Combine(outFolder, "plane-" + CreationDate.ToString("HH-mm") + Path.GetFileNameWithoutExtension(_path) + ".mp4") };
                var jumpOutputFile = new MediaFile { Filename = Path.Combine(outFolder, "jump-" + CreationDate.ToString("HH-mm") + Path.GetFileNameWithoutExtension(_path) + ".mp4") };

                using (var engine = new Engine())
                {
                    engine.GetMetadata(inputFile);

                    var options = new ConversionOptions();

                    var duration = Duration - jumpStartTime;
                    options.CutMedia(TimeSpan.FromSeconds(Convert.ToDouble(jumpStartTime)), TimeSpan.FromSeconds(Convert.ToDouble(duration)));

                    _loadingService.AddLoadingStatus($"Creation de la vidéo {Path.GetFileNameWithoutExtension(jumpOutputFile.Filename)}...");
                    engine.ConversionCompleteEvent += (sender, args) =>
                    {
                        _loadingService.RemoveLoadingStatus( $"Creation de la vidéo {Path.GetFileNameWithoutExtension(jumpOutputFile.Filename)}...");
                    };
                    engine.Convert(inputFile, jumpOutputFile, options);
                }

                using (var engine = new Engine())
                {
                    engine.GetMetadata(inputFile);

                    var options = new ConversionOptions();

                    options.CutMedia(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(Convert.ToDouble(jumpStartTime)));

                    _loadingService.AddLoadingStatus($"Creation de la vidéo {Path.GetFileNameWithoutExtension(planeOutputFile.Filename)}...");
                    engine.ConversionCompleteEvent += (sender, args) =>
                    {
                        _loadingService.RemoveLoadingStatus($"Creation de la vidéo {Path.GetFileNameWithoutExtension(planeOutputFile.Filename)}...");
                    };
                    engine.Convert(inputFile, planeOutputFile, options);
                }
            });
        }
    }
}
