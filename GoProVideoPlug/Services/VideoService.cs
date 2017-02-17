using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Diagnostics;
using GoProVideoPlug.Helpers;
using GoProVideoPlug.IServices;
using GoProVideoPlug.Models;
using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using Debug = System.Diagnostics.Debug;

namespace GoProVideoPlug.Services
{
    public class VideoService
    {
        private List<VisionServiceClient> _clients;
        private int _currentClientIndex;
        private ILoadingService _loadingService;

        public VideoService(params string[] visionApiSubscriptionKeys)
        {
            InitVisionServiceClients(visionApiSubscriptionKeys);
            _loadingService = DependencyInjectionUtil.Resolve<ILoadingService>();
        }

        public async Task<long?> GetStartFrame(Video video)
        {
            var nextFrame = video.FrameCount / 2;
            long? startframe = null;
            var precision = nextFrame;

            while (precision > App.DetectionPrecision)
            {
                Debug.WriteLine("Analyse : " + nextFrame / video.FrameRate + " seconds");

                using (var frame = await video.GetFrame(nextFrame))
                using (MemoryStream stream = new MemoryStream())
                {
                    frame.Save(stream, ImageFormat.Jpeg);
                    stream.Seek(0, SeekOrigin.Begin);
                    var tags = await GetTags(stream);

                    if (tags.Any(t => t.Confidence > 0.8 && t.Name.ToLower() == "outdoor"))
                    {
                        startframe = nextFrame;
                        nextFrame = nextFrame - precision / 2;
                        precision = precision / 2;
                    }
                    else
                    {
                        nextFrame = nextFrame + precision / 2;
                        precision = precision / 2;
                    }
                    stream.Close();
                }
            }
            return startframe;
        }

        public Task Copy(Video video, string newDirectory)
        {
            return Task.Run(() =>
            {
                _loadingService.AddLoadingStatus($"Copie de {Path.GetFileNameWithoutExtension(video.Path)} vers le dossier temporaire...");
                var newPath = Path.Combine(newDirectory, Path.GetFileName(video.Path));
                File.Copy(video.Path, newPath);
                video.Path = newPath;
                video.InitReader();
                _loadingService.RemoveLoadingStatus($"Copie de {Path.GetFileNameWithoutExtension(video.Path)} vers le dossier temporaire...");
            });
        }
        public Task Cut(Video video, long jumpStartTime, string outRootPath)
        {
            return Task.Run(() =>
            {
                var inputFile = new MediaFile { Filename = video.Path };

                var outFolder = Path.Combine(outRootPath, video.CreationDate.Year.ToString(), video.CreationDate.ToString("MMMM"), video.CreationDate.Day.ToString());

                if (!Directory.Exists(outFolder))
                    Directory.CreateDirectory(outFolder);

                var planeOutputFile = new MediaFile { Filename = Path.Combine(outFolder, "plane-" + video.CreationDate.ToString("HH-mm") + Path.GetFileNameWithoutExtension(video.Path) + ".mp4") };
                var jumpOutputFile = new MediaFile { Filename = Path.Combine(outFolder, "jump-" + video.CreationDate.ToString("HH-mm") + Path.GetFileNameWithoutExtension(video.Path) + ".mp4") };

                using (var engine = new Engine())
                {
                    engine.GetMetadata(inputFile);

                    var options = new ConversionOptions();

                    var duration = video.Duration - jumpStartTime;
                    options.CutMedia(TimeSpan.FromSeconds(Convert.ToDouble(jumpStartTime)), TimeSpan.FromSeconds(Convert.ToDouble(duration)));

                    _loadingService.AddLoadingStatus($"Creation de la vidéo {Path.GetFileNameWithoutExtension(jumpOutputFile.Filename)}...");
                    engine.ConversionCompleteEvent += (sender, args) =>
                    {
                        _loadingService.RemoveLoadingStatus($"Creation de la vidéo {Path.GetFileNameWithoutExtension(jumpOutputFile.Filename)}...");
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

        #region Private methods

        private async Task<List<Tag>> GetTags(Stream image)
        {
            var res = await _clients[_currentClientIndex].GetTagsAsync(image);
            _currentClientIndex = _currentClientIndex > _clients.Count - 1 ? 0 : _currentClientIndex;
            return res.Tags.ToList();
        }
        private void InitVisionServiceClients(params string[] visionApiSubscriptionKeys)
        {
            _clients = new List<VisionServiceClient>();
            _currentClientIndex = 0;
            foreach (var key in visionApiSubscriptionKeys)
            {
                _clients.Add(new VisionServiceClient(key));
            }
        }

        #endregion
    }
}
