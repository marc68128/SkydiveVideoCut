using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Diagnostics;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using SkydivingVideoCut.Models;
using Debug = System.Diagnostics.Debug;

namespace SkydivingVideoCut.Services
{
    public class VisionService
    {
        private readonly List<VisionServiceClient> _clients;
        private int _currentClientIndex;

        public VisionService(params string[] subscriptionKeys)
        {
            _clients = new List<VisionServiceClient>();
            _currentClientIndex = 0;
            foreach (var key in subscriptionKeys)
            {
                _clients.Add(new VisionServiceClient(key));
            }
        }
        public async Task<List<Tag>> GetTags(Stream image)
        {
            var res = await _clients[_currentClientIndex++].GetTagsAsync(image);
            _currentClientIndex = _currentClientIndex > _clients.Count - 1 ? 0 : _currentClientIndex;
            return res.Tags.ToList();
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
    }
}
