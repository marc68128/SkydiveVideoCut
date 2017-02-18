using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Diagnostics;
using Analyser.Models;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using Debug = System.Diagnostics.Debug;

namespace SkydivingVideoCut.Services
{
    public class VisionService
    {
        private Object _locker = new Object();
        private readonly List<Tuple<DateTime, VisionServiceClient>> _clients;
        private int _currentClientIndex;

        public VisionService(params string[] subscriptionKeys)
        {
            _clients = new List<Tuple<DateTime, VisionServiceClient>>();
            _currentClientIndex = 0;
            foreach (var key in subscriptionKeys)
            {
                _clients.Add(new Tuple<DateTime, VisionServiceClient>(DateTime.MinValue, new VisionServiceClient(key)));
            }
        }
        public async Task<List<Tag>> GetTags(Stream image)
        {
            var index = _currentClientIndex;
            _currentClientIndex++;
            _currentClientIndex = _currentClientIndex > _clients.Count - 1 ? 0 : _currentClientIndex;

            bool wait;

            lock (_locker)
            {
                wait = !(DateTime.Now.AddSeconds(-4) > _clients[index].Item1);
                if (!wait)
                    _clients[index] = new Tuple<DateTime, VisionServiceClient>(DateTime.Now, _clients[index].Item2);
            }

            while (wait)
            {
                await Task.Delay(1000);
                lock (_locker)
                {
                    wait = !(DateTime.Now.AddSeconds(-4) > _clients[index].Item1);
                    if (!wait)
                        _clients[index] = new Tuple<DateTime, VisionServiceClient>(DateTime.Now, _clients[index].Item2);
                }
            }
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Start NetworkRequest");
            Console.ForegroundColor = ConsoleColor.White;
            var res = await _clients[index].Item2.GetTagsAsync(image);


            return res.Tags.ToList();
        }
    }
}
