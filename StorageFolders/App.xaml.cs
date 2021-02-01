using LightForms;
using LightForms.Services;
using Plugin.DeviceStorage;
using StorageFolders.ViewModels;
using StorageFolders.Views;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace StorageFolders
{
    public partial class App : LightFormsApplication
    {

        public App(IPlatformInitializer initializer = null) : base(initializer) { }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            InitializeComponent();
            Initialize<MainPage, MainViewModel>("/main");
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

        }
    }
}
