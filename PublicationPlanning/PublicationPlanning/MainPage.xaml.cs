using PublicationPlanning.Services;
using PublicationPlanning.StoredModels;
using PublicationPlanning.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace PublicationPlanning
{
    public partial class MainPage : ContentPage
    {
        private readonly IImageInfoService service;
        private const int imageSizeRequest = 128;

        public MainPage(IImageInfoService service)
        {
            this.service = service;

            InitializeComponent();

            //new TestPictures(service).LoadTestDataToStorage();

            ShowImages();
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            await PickImageAsync();
        }

        private async Task PickImageAsync()
        {
            try
            {
                var result = await MediaPicker.PickPhotoAsync(null);
                if (result != null)
                {
                    int id = await service.InsertFirst(new ImageInfoViewModel()
                    {
                        ImageRef = result.FullPath,
                        SourceType = ImageSourceType.FilePath
                    });

                    ImageInfoViewModel image = await service.Get(id);

                    var stream = await result.OpenReadAsync();
                    AddImageToLayout(image.ImageSource, image.Order);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", ex.Message, "Close");
            }
        }

        private async Task ShowImages()
        {
            var allImages = await service.GetPage(0, 100);

            foreach (var image in allImages.OrderBy(x => x.Order))
            {
                AddImageToLayout(image.ImageSource, image.Order);
            }

            activityIndicator.IsRunning = false;
            activityIndicator.IsVisible = false;
        }

        private void AddImageToLayout(ImageSource source, int order = 0)
        {
            Image image = new Image
            {
                Source = source,
                Margin = new Thickness(5),
                VerticalOptions = LayoutOptions.CenterAndExpand,
                WidthRequest = imageSizeRequest,
                HeightRequest = imageSizeRequest,
                Aspect = Aspect.AspectFill
            };
            flexLayout.Children.Add(image);
            FlexLayout.SetBasis(image, new FlexBasis(0.33f, true));

            FlexLayout.SetOrder(image, order);

            //FlexLayout.SetGrow(image, 0.33f);
            //FlexLayout.SetShrink(image, 0.33f);
        }

    }
}
