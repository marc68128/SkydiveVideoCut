using System.ComponentModel;
using System.Runtime.CompilerServices;
using GoProVideoPlug.Helpers;
using GoProVideoPlug.IServices;

namespace GoProVideoPlug.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        protected ILoadingService LoadingService;

        public BaseViewModel()
        {
            LoadingService = DependencyInjectionUtil.Resolve<ILoadingService>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
