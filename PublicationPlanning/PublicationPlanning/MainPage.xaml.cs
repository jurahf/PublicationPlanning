using PublicationPlanning.MainPageViewFeature.DragAndDrop;
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
    public partial class MainPage : ContentPage, ISelectImageContext, IDragDropContext
    {
        private readonly IImageInfoService service;
        private int imageSizeRequest = 128;
        private const int spacePixel = 5;
        List<(int order, int modelId, View view)> flexLayoutCells = new List<(int, int, View)>();

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

        private async void btnRefresh_Clicked(object sender, EventArgs e)
        {
            await ShowImages();
        }

        private async void btnRemove_Clicked(object sender, EventArgs e)
        {
            if (selectedImage == null)
                return;

            bool approve = await DisplayAlert("Delete", "Delete this image?", "Delete", "Cancel");

            if (!approve)
                return;

            int deletedId = selectedImage.ImageInfoId;

            await service.Delete(deletedId);
            (_, _, View view) = flexLayoutCells.FirstOrDefault(x => x.modelId == deletedId);

            if (view != null)
            {
                flexLayout.Children.Remove(view);
                flexLayoutCells = flexLayoutCells.Where(x => x.modelId != deletedId).ToList();
            }

            ClearSelection();
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

                if (!allImages.Any())
                {
                    // TODO: заставку для пустого экрана
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
            double widthBase = 
                Application.Current?.MainPage?.Width 
                ?? (DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density);
            imageSizeRequest = (int)(widthBase * 0.33) - (spacePixel * 2) + 1;

            // картинка
            Image image = new Image
            {
                Source = imageInfo.ImageSource,
                Aspect = Aspect.AspectFill,

                VerticalOptions = LayoutOptions.CenterAndExpand,
                WidthRequest = imageSizeRequest,
                HeightRequest = imageSizeRequest,
            };

            // два посадочных места для картинок
            Frame leftLending = new Frame()
            {
                VerticalOptions = LayoutOptions.CenterAndExpand,
                WidthRequest = imageSizeRequest / 2,
                HeightRequest = imageSizeRequest,
                BackgroundColor = Color.Transparent
            };

            Frame rightLending = new Frame()
            {
                VerticalOptions = LayoutOptions.CenterAndExpand,
                WidthRequest = imageSizeRequest / 2,
                HeightRequest = imageSizeRequest,
                BackgroundColor = Color.Transparent
            };

            // располагаются в шаблоне друг на друге
            AbsoluteLayout absolut = new AbsoluteLayout();
            absolut.Children.Add(image);
            AbsoluteLayout.SetLayoutBounds(image, new Rectangle(0, 0, imageSizeRequest, imageSizeRequest));
            absolut.Children.Add(leftLending);
            AbsoluteLayout.SetLayoutBounds(leftLending, new Rectangle(0, 0, imageSizeRequest / 2, imageSizeRequest));
            absolut.Children.Add(rightLending);
            AbsoluteLayout.SetLayoutBounds(rightLending, new Rectangle(imageSizeRequest / 2, 0, imageSizeRequest / 2, imageSizeRequest));

            // и все вместе помещены во фрейм
            Frame frame = new Frame()
            {
                Content = absolut,

                VerticalOptions = LayoutOptions.CenterAndExpand,
                WidthRequest = imageSizeRequest,
                HeightRequest = imageSizeRequest,
            };

            SetUnselectStyle(frame);

            // жест нажатия - выделение картинки
            frame.GestureRecognizers.Add(
                new TapGestureRecognizer()
                {
                    Command = new SelectImageCommand(),
                    CommandParameter = new SelectImageCommandParameter(this, new ImageModelAndControl(imageInfo.Id, frame))
                });

            // перетаскивание
            var dragRecognizer = new DragGestureRecognizer()
            {
                DropCompletedCommand = new DragAndDropCompletedCommand(),
                DropCompletedCommandParameter = new DragAndDropCompletedParameter(this, new ImageModelAndControl(imageInfo.Id, frame))
            };
            dragRecognizer.DragStarting += (s, a) =>
            {
                a.Data.Text = "";
                //a.Data.Image = image.Source;
                a.Data.Properties.Add("Id", imageInfo.Id);
            };

            frame.GestureRecognizers.Add(dragRecognizer);


            // перетаскивание чего-то над объектом - реакция нижнего объекта
            leftLending.GestureRecognizers.Add(
                new DropGestureRecognizer()
                {
                    DragLeaveCommand = new DragLeaveCommand(),
                    DragLeaveCommandParameter = new DropOverCommandParameter(
                        this,
                        new DragDropInfo(imageInfo.Id, leftLending, DropOnObjectDirection.ToLeft)),
                    DragOverCommand = new DropOverCommand(),
                    DragOverCommandParameter = new DropOverCommandParameter(
                        this, 
                        new DragDropInfo(imageInfo.Id, leftLending, DropOnObjectDirection.ToLeft))
                });

            rightLending.GestureRecognizers.Add(
                new DropGestureRecognizer()
                {
                    DragLeaveCommand = new DragLeaveCommand(),
                    DragLeaveCommandParameter = new DropOverCommandParameter(
                        this,
                        new DragDropInfo(imageInfo.Id, rightLending, DropOnObjectDirection.ToRight)),
                    DragOverCommand = new DropOverCommand(),
                    DragOverCommandParameter = new DropOverCommandParameter(
                        this,
                        new DragDropInfo(imageInfo.Id, rightLending, DropOnObjectDirection.ToRight))
                });

            flexLayout.Children.Add(frame);
            FlexLayout.SetBasis(frame, new FlexBasis(0.33f, true));

            FlexLayout.SetOrder(frame, imageInfo.Order);

            flexLayoutCells.Add((imageInfo.Order, imageInfo.Id, frame));
        }

        #region DragDropContext

        private List<DragDropInfo> activeDropLanding = new List<DragDropInfo>();
        private object dropLock = new object();

        public void OnDragOver(DragDropInfo landing)
        {
            lock (dropLock)
            {
                if (!activeDropLanding.Any(x => x.ImageInfoId == landing.ImageInfoId && x.Direction == landing.Direction))
                {
                    SetActiveDropStyle(landing.Control);
                    activeDropLanding.Add(landing);
                }
            }
        }

        public void OnDragLeave(DragDropInfo landing)
        {
            lock (dropLock)
            {
                foreach (var frame in activeDropLanding
                    .Where(x => x.ImageInfoId == landing.ImageInfoId && x.Direction == landing.Direction))
                {
                    SetUnactiveDropStyle(frame.Control);
                }

                activeDropLanding = activeDropLanding
                    .Where(x => x.ImageInfoId != landing.ImageInfoId && x.Direction == landing.Direction)
                    .ToList();
            }
        }

        public async void CompleteDrop(ImageModelAndControl src)
        {
            // src перемещаем слева или справа от DragDropInfo (надеюсь, он там один или два аналогичных)
            if (!activeDropLanding.Any())
                return;

            var activeLandingInfo = activeDropLanding.First();      // ?!
            ImageInfoViewModel dragged = await service.Get(src.ImageInfoId);
            ImageInfoViewModel landing = await service.Get(activeLandingInfo.ImageInfoId);

            int startOrder = dragged.Order;
            int endOrder;

            if (dragged.Order < landing.Order)
            {
                endOrder = activeLandingInfo.Direction == DropOnObjectDirection.ToLeft
                    ? landing.Order - 1
                    : landing.Order;
            }
            else if (dragged.Order > landing.Order)
            {
                endOrder = activeLandingInfo.Direction == DropOnObjectDirection.ToLeft
                    ? landing.Order
                    : landing.Order + 1;
            }
            else
            {
                endOrder = dragged.Order;
            }

            if (startOrder != endOrder)
            {
                // перемещаем в базе
                await service.MoveOrder(dragged, endOrder);

                // и на форме
                MoveImageOnLayout(dragged.Id, startOrder, endOrder);
            }

            // отменяем выделения всех областей для посадки
            foreach (var active in activeDropLanding)
            {
                SetUnactiveDropStyle(active.Control);
            }

            activeDropLanding.Clear();
        }

        private void MoveImageOnLayout(int draggedId, int oldIndex, int newIndex)
        {
            var movedList = flexLayoutCells
                .Where(x =>
                    x.order >= Math.Min(oldIndex, newIndex)
                    && x.order <= Math.Max(oldIndex, newIndex))
                .ToList();

            int moveDirection = oldIndex < newIndex ? -1 : 1;
            foreach (var pair in movedList)
            {
                int newViewIndex = pair.modelId == draggedId
                    ? newIndex
                    : pair.order + moveDirection;

                flexLayoutCells.Remove(pair);
                flexLayoutCells.Add((newViewIndex, pair.modelId, pair.view));
                FlexLayout.SetOrder(pair.view, newViewIndex);
            }
        }

        private void SetActiveDropStyle(View frame)
        {
            frame.BackgroundColor = Color.FromRgba(200, 200, 0, 0.4);
        }

        private void SetUnactiveDropStyle(View frame)
        {
            frame.BackgroundColor = Color.Transparent;
        }

        # endregion DragDropContext


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
            frame.Padding = new Thickness(spacePixel);
            frame.Margin = new Thickness(0);
        }

        private void SetUnselectStyle(Frame frame)
        {
            if (frame == null)
                return;

            frame.BorderColor = Color.Default;
            frame.BackgroundColor = Color.Default;
            frame.Margin = new Thickness(spacePixel);
            frame.Padding = new Thickness(0);
        }

        private void EnableImageOperationButtons(bool isEnable)
        {
            btnRemove.IsEnabled = isEnable;
        }

        #endregion

    }
}
