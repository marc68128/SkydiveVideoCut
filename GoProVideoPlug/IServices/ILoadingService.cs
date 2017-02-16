using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoProVideoPlug.IServices
{
    public interface ILoadingService
    {
        string CurrentStatus { get; }
        bool IsLoading { get; }
        void RemoveLoadingStatus(string status);
        void AddLoadingStatus(string status);

        event EventHandler<string> CurrentStatusChanged;
        event EventHandler<bool> IsLoadingChanged;
    }
}
