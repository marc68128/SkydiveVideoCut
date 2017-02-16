using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Video.FFMPEG;

namespace GoProVideoPlug.Models
{
    public class Video
    {
        private string _path;
        private readonly VideoFileReader _reader = new VideoFileReader();
        public int FrameRate { get; set; }
        public long FrameCount { get; set; }
        public long CurrentFrame { get; private set; }

        public Video(string path)
        {
            _path = path;
            _reader.Open(path);
            FrameCount = _reader.FrameCount;
            FrameRate = _reader.FrameRate;
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
                var newPath = Path.Combine(newDirectory, Path.GetFileName(_path));
                File.Copy(_path, newPath);
                _path = newPath;
                _reader.Close();
                _reader.Open(_path);
                CurrentFrame = 0;
            });
        }
    }
}
