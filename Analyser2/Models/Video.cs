using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Video.FFMPEG;
using Microsoft.ProjectOxford.Vision.Contract;

namespace Analyser2.Models
{
    public class Video
    {
        private readonly VideoFileReader _reader = new VideoFileReader();

        public string Name { get; set; }
        public string FullPath { get; set; }

        public int FrameRate { get; set; }
        public long FrameCount { get; }
        public long CurrentFrame { get; private set; }
        public long Duration { get; set; }

        public Dictionary<double, List<Tag>> Tags { get; set; }

        public Video(string path)
        {
            //FullPath = path;
            //_reader.Open(path);
            //Name = Path.GetFileName(path);
            //FrameCount = _reader.FrameCount;
            //FrameRate = _reader.FrameRate;
            //Duration = FrameCount / FrameRate;
            Tags = new Dictionary<double, List<Tag>>();
        }

        public Task SkipFrame(int count = 1)
        {
            return Task.Run(() =>
            {
                for (int i = 0; i < count; i++)
                {
                    if (CurrentFrame < FrameCount)
                    {
                        _reader.ReadVideoFrame().Dispose();
                        CurrentFrame++;
                    }
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
            if (frame < CurrentFrame)
            {
                _reader.Close();
                _reader.Open(FullPath);
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
    }
}
