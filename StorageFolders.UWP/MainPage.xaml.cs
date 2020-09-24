namespace StorageFolders.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();

            LoadApplication(new StorageFolders.App());
        }
    }
}