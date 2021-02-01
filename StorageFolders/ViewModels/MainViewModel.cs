using LightForms.ViewModels;
using Plugin.DeviceStorage;
using System.Collections.Generic;

namespace StorageFolders.ViewModels
{
    public class MainViewModel : ViewModelBase
    {

        #region Notified Property Files
        /// <summary>
        /// Files
        /// </summary>
        private IEnumerable<IStorageFile> files;
        public IEnumerable<IStorageFile> Files
        {
            get { return files; }
            set { files = value; OnPropertyChanged(); }
        }
        #endregion

        public override async void Appearing(string route, object data)
        {
            base.Appearing(route, data);
            var downloads = await CrossDeviceStorage.Current.Pictures;
            if (downloads == null) return;
            var files = await downloads.GetFiles();
            Files = files;
        }

    }
}