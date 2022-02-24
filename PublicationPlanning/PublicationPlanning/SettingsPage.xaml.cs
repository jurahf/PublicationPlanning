using PublicationPlanning.Services;
using PublicationPlanning.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PublicationPlanning
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        private readonly ISettingsService settingsService;
        private SettingsViewModel settings;

        public int ColumnsCount
        {
            get { return settings.ColumnsCount; }
            set
            {
                if (ColumnsCount != value)
                {
                    settings.ColumnsCount = value;
                    OnPropertyChanged(nameof(ColumnsCount));
                }
            }
        }

        public bool UseImageResize
        {
            get { return settings.ResizeImages; }
            set
            {
                if (UseImageResize != value)
                {
                    settings.ResizeImages = value;
                    OnPropertyChanged(nameof(UseImageResize));
                }
            }
        }

        public int ImageResizeWidth
        {
            get { return settings.ImageResizeWidth; }
            set
            {
                if (ImageResizeWidth != value)
                {
                    settings.ImageResizeWidth = value;
                    OnPropertyChanged(nameof(ImageResizeWidth));
                }
            }
        }

        public int ImageResizeHeight
        {
            get { return settings.ImageResizeHeight; }
            set
            {
                if (ImageResizeHeight != value)
                {
                    settings.ImageResizeHeight = value;
                    OnPropertyChanged(nameof(ImageResizeHeight));
                }
            }
        }

        public int ImageCompressQuality
        {
            get { return settings.ImageCompressQuality; }
            set
            {
                if (ImageCompressQuality != value)
                {
                    settings.ImageCompressQuality = value;
                    OnPropertyChanged(nameof(ImageCompressQuality));
                }
            }
        }

        public int PageSize
        {
            get { return settings.PageSize; }
            set
            {
                if (PageSize != value)
                {
                    settings.PageSize = value;
                    OnPropertyChanged(nameof(PageSize));
                }
            }
        }

        public int ImageSpacingPixels
        {
            get { return settings.ImageSpacingPixels; }
            set
            {
                if (ImageSpacingPixels != value)
                {
                    settings.ImageSpacingPixels = value;
                    OnPropertyChanged(nameof(ImageSpacingPixels));
                }
            }
        }

        public SettingsPage(ISettingsService settingsService, SettingsViewModel settings)
        {
            this.settingsService = settingsService;
            this.settings = settings;
            this.Disappearing += SettingsPage_Disappearing;

            InitializeComponent();
        }

        private async void SettingsPage_Disappearing(object sender, EventArgs e)
        {
            await settingsService.Update(settings.Id, settings);
        }
    }
}