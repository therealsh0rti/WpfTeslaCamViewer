using System.Windows;
using LibVLCSharp.Shared;
using Microsoft.Extensions.DependencyInjection;
using WpfTeslaCamViewer.Factories;
using WpfTeslaCamViewer.ViewModels;

namespace WpfTeslaCamViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ServiceProvider serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            services.AddSingleton<ILibVlcFactory, LibVlcFactory>();
            services.AddSingleton<IMediaPlayerFactory, MediaPlayerFactory>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<MainWindow>();
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            var mainWindow = serviceProvider.GetService<MainWindow>();
            mainWindow?.Show();
        }
    }
}
