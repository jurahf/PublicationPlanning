using PublicationPlanning.MainPageViewFeature.SelectImage;
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
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace PublicationPlanning
{
    public partial class MainPage : ContentPage, ISelectImageContext
    {
        private readonly IImageInfoService service;
        private const int imageSizeRequest = 128;
        List<(int order, int modelId, BindableObject view)> flexLayoutCells = new List<(int, int, BindableObject)>();

        public MainPage(IImageInfoService service)
        {
            this.service = service;

            InitializeComponent();

            //new TestPictures(service).LoadTestDataToStorage();

            ShowImages();
        }

        private async void btnAddPhoto_Clicked(object sender, EventArgs e)
        {
            await PickImageAsync();
        }

        private async void btnToLeft_Clicked(object sender, EventArgs e)
        {
            await MoveSelectedImage((x) => x - 1);
        }

        private async void btnToRight_Clicked(object sender, EventArgs e)
        {
            await MoveSelectedImage((x) => x + 1);
        }

        private async Task MoveSelectedImage(Func<int, int> indexFunc)
        {
            ImageModelAndControl selection = GetSelection();

            if (selection == null)
                return;

            ImageInfoViewModel model = await service.Get(selection.ImageInfoId);

            if (model == null)
                return;

            int oldIndex = model.Order;
            int newIndex = indexFunc(model.Order);

            // двигаем в базе
            await service.MoveOrder(model, newIndex);

            // двигаем в представлении
            var movedList = flexLayoutCells
                .Where(x =>
                    x.order >= Math.Min(oldIndex, newIndex)
                    && x.order <= Math.Max(oldIndex, newIndex))
                .ToList();

            int moveDirection = oldIndex < newIndex ? -1 : 1;
            foreach (var pair in movedList)
            {
                int newViewIndex = pair.modelId == selectedImage.ImageInfoId
                    ? newIndex
                    : pair.order + moveDirection;

                flexLayoutCells.Remove(pair);
                flexLayoutCells.Add((newViewIndex, pair.modelId, pair.view));
                FlexLayout.SetOrder(pair.view, newViewIndex);
            }
        }

        private async void btnRefresh_Clicked(object sender, EventArgs e)
        {
            await ShowImages();
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
                    AddImageToLayout(image);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", ex.Message, "Close");
            }
        }

        private async Task ShowImages()
        {
            activityIndicator.IsRunning = true;
            activityIndicator.IsVisible = true;
            ClearSelection();

            try
            {
                flexLayout.Children.Clear();
                flexLayoutCells.Clear();
                var allImages = await service.GetPage(0, 100);

                foreach (var image in allImages.OrderBy(x => x.Order))
                {
                    AddImageToLayout(image);
                }
            }
            finally
            {
                activityIndicator.IsRunning = false;
                activityIndicator.IsVisible = false;
            }
        }

        private void AddImageToLayout(ImageInfoViewModel imageInfo)
        {
            Image image = new Image
            {
                Source = imageInfo.ImageSource,
                Aspect = Aspect.AspectFill,

                VerticalOptions = LayoutOptions.CenterAndExpand,
                WidthRequest = imageSizeRequest,
                HeightRequest = imageSizeRequest,
            };

            Frame frame = new Frame()
            {
                Content = image,

                VerticalOptions = LayoutOptions.CenterAndExpand,
                WidthRequest = imageSizeRequest,
                HeightRequest = imageSizeRequest,
            };

            SetUnselectStyle(frame);

            // жест нажатия - выделение картинки
            image.GestureRecognizers.Add(
                new TapGestureRecognizer()
                {
                    Command = new SelectImageCommand(),
                    CommandParameter = new SelectImageCommandParameter(this, new ImageModelAndControl(imageInfo.Id, frame))
                });

            // TODO: перетаскивание
            //image.GestureRecognizers.Add(
            //    new DragGestureRecognizer()
            //    {
            //        DragStartingCommand = new DragAndDropStartCommand(),
            //    },
            //    new DropGestureRecognizer()
            //    {
            //        //DragOverCommand
            //    });

            flexLayout.Children.Add(frame);
            FlexLayout.SetBasis(frame, new FlexBasis(0.33f, true));

            FlexLayout.SetOrder(frame, imageInfo.Order);

            flexLayoutCells.Add((imageInfo.Order, imageInfo.Id, frame));
        }



        #region ImageSelectionContext

        private ImageModelAndControl selectedImage = null; // без multiselect

        public void ClearSelection()
        {
            if (selectedImage != null)
            {
                EnableImageOperationButtons(false);
                SetUnselectStyle(selectedImage.Control as Frame);
                selectedImage = null;
            }
        }

        public void SetSelection(ImageModelAndControl selected)
        {
            ClearSelection();

            if (selected == null)
                return;

            Frame frame = selected.Control as Frame;
            SetSelectStyle(frame);

            selectedImage = selected;

            EnableImageOperationButtons(true);
        }

        public ImageModelAndControl GetSelection()
        {
            return selectedImage;
        }

        private void SetSelectStyle(Frame frame)
        {
            if (frame == null)
                return;

            frame.BorderColor = Color.Red;
            frame.BackgroundColor = Color.Red;
            frame.Padding = new Thickness(5);
            frame.Margin = new Thickness(0);
        }

        private void SetUnselectStyle(Frame frame)
        {
            if (frame == null)
                return;

            frame.BorderColor = Color.Default;
            frame.BackgroundColor = Color.Default;
            frame.Margin = new Thickness(5);
            frame.Padding = new Thickness(0);
        }

        private void EnableImageOperationButtons(bool isEnable)
        {
            btnToLeft.IsEnabled = isEnable;
            btnToRight.IsEnabled = isEnable;
        }

        #endregion

    }
}
