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
        private readonly VideoFileReader _reader = new VideoFileReader();

        public string Path { get; set; }
        public int FrameRate { get; set; }
        public long FrameCount { get; set; }
        public long Duration { get; set; }
        public long CurrentFrame { get; private set; }
        public DateTime CreationDate { get; set; }

        public Video(string path)
        {
            Path = path;
            CreationDate = File.GetCreationTime(Path);
            
            InitReader();
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
                _reader.Open(Path);
                CurrentFrame = 0;
            }

            if (frame < CurrentFrame)
            {
                _reader.Close();
                _reader.Open(Path);
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

        public void InitReader()
        {
            if(_reader.IsOpen)
                _reader.Close();
            _reader.Open(Path);
            FrameCount = _reader.FrameCount;
            FrameRate = _reader.FrameRate;
            Duration = FrameCount / FrameRate;
        }
    }
}
