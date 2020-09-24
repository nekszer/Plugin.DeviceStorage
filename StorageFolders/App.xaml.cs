using LightForms;
using LightForms.Services;
using Plugin.DeviceStorage;
using StorageFolders.ViewModels;
using StorageFolders.Views;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace StorageFolders
{
    public partial class App : LightFormsApplication, IPermissionRequest
    {

        public App(IPlatformInitializer initializer = null) : base(initializer) { }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            InitializeComponent();
            Initialize<MainPage, MainViewModel>("/main");
            CrossDeviceStorage.Current.SetPermissionRequest(this);
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        protected override void OnSleep()
        {
            base.OnSleep();
        }

        protected override void OnResume()
        {
            base.OnSleep();
        }

        protected override void Routes(IRoutingService routingservice)
        {
            // set routes 
            // routingservice.Route<View,ViewModel>("/routename");
        }

        public async Task<bool> Request()
        {
            var read = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            var write = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
            if (read != PermissionStatus.Granted)
                read = await Permissions.RequestAsync<Permissions.StorageRead>();
            if (write != PermissionStatus.Granted)
                write = await Permissions.RequestAsync<Permissions.StorageWrite>();
            if (read != PermissionStatus.Granted || write != PermissionStatus.Granted) return false;
            return true;
        }
    }
}
