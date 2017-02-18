using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Analyser.Helpers;
using Analyser.Models;
using Newtonsoft.Json;
using SkydivingVideoCut.Services;

namespace Analyser
{
    class Program
    {
        private static VisionService _visionService = new VisionService("1d7e166bae1e4ea2ad0e0017f9cb44ab");

        static void Main(string[] args)
        {
            if (args == null || args.Length != 1)
                return;

            var path = args.First();

            FileAttributes attr = File.GetAttributes(path);

            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                //It's a directory
                var videos = IOExtensions.VideoExtensions.SelectMany(f => IOExtensions.GetFilesRecursiv(path, f)).Where(p => !p.StartsWith(".")).Distinct();
                foreach (var v in videos)
                {
                    AnalyseVideo(v).ContinueWith(t =>
                    {
                        var video = t.Result;
                        var output = Path.Combine(@"C:\Users\Marc\Desktop\Analyse", Path.GetFileNameWithoutExtension(video.Name) + ".json");
                        File.Open(output, FileMode.Create).Dispose();
                        File.WriteAllText(output, JsonConvert.SerializeObject(video), Encoding.UTF8);
                    }).Wait();
                }
            }
            else
            {
                AnalyseVideo(path).ContinueWith(t =>
                {
                    var video = t.Result;
                    var output = Path.Combine(@"C:\Users\Marc\Desktop\Analyse", Path.GetFileNameWithoutExtension(video.Name) + ".json");
                    File.Open(output , FileMode.Create).Dispose();
                    File.WriteAllText(output , JsonConvert.SerializeObject(video), Encoding.UTF8);
                });
            }

            Console.ReadLine();
        }

        private static Task<Video> AnalyseVideo(string path)
        {
            var tasks = new List<Task>();
            var video = new Video(path);
            while (video.CurrentFrame < video.FrameCount -1)
            {

                var time = (double)video.CurrentFrame / video.FrameRate;
                Console.WriteLine($"Analyse {video.Name} at time {time} s");

                var frameTask = video.GetFrame();

                tasks.Add(frameTask.ContinueWith(task =>
                {
                    var frame = task.Result;
                    MemoryStream stream = new MemoryStream();
                    frame.Save(stream, ImageFormat.Jpeg);
                    stream.Seek(0, SeekOrigin.Begin);
                    tasks.Add(_visionService.GetTags(stream).ContinueWith(t =>
                    {
                        video.Tags.Add(time, t.Result);
                        stream.Close();
                        stream.Dispose();
                    }));
                    frame.Dispose();
                }));

                frameTask.Wait();
                video.SkipFrame(video.FrameRate).Wait();
            }
            return Task.WhenAll(tasks).ContinueWith(task => video);
        }
    }
}