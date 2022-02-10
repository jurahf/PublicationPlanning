using System;
using Microsoft.Extensions.DependencyInjection;
using Xamarin.Forms;
using PublicationPlanning.Converters;
using PublicationPlanning.Repositories;
using PublicationPlanning.Services;
using PublicationPlanning.StoredModels;
using PublicationPlanning.ViewModels;


namespace PublicationPlanning
{
    public partial class App : Application
    {
        private static IServiceProvider serviceProvider { get; set; }

        public App(ServiceCollection services)
        {
            InitializeComponent();

            SetupServices(services);

            var navigationPage = new NavigationPage(
                new MainPage(
                    serviceProvider.GetService<IImageInfoService>(),
                    serviceProvider.GetService<ISettingsService>()
                    ));

            navigationPage.BarBackgroundColor = Color.FromHex("2196F3");
            navigationPage.BarTextColor = Color.White;

            MainPage = navigationPage;
        }

        void SetupServices(ServiceCollection services)
        {
            services.AddSingleton<IEntityConverter<Settings, SettingsViewModel>, SettingsConverter>();
            services.AddSingleton<ISettingsRepository, SettingsFileRepository>();
            services.AddSingleton<ISettingsService, SettingsService>();

            services.AddSingleton<IEntityConverter<ImageInfo, ImageInfoViewModel>, ImageInfoConverter>();
            services.AddSingleton<IImageInfoRepository, ImageInfoFileRepository>();
            services.AddSingleton<IImageInfoService, ImageInfoService>();

            serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
