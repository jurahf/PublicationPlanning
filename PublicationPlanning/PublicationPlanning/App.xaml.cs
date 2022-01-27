using Microsoft.Extensions.DependencyInjection;
using PublicationPlanning.Converters;
using PublicationPlanning.Repositories;
using PublicationPlanning.Services;
using PublicationPlanning.StoredModels;
using PublicationPlanning.ViewModels;
using System;
using Xamarin.Forms;
//using Xamarin.Forms.Xaml;

namespace PublicationPlanning
{
    public partial class App : Application
    {
        private static IServiceProvider serviceProvider { get; set; }

        public App()
        {
            InitializeComponent();

            SetupServices();

            MainPage = new MainPage(serviceProvider.GetService<IImageInfoService>());
        }

        void SetupServices()
        {
            var services = new ServiceCollection();

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
