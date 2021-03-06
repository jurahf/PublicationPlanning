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
        private readonly IImageInfoService imageService;
        private readonly ISettingsService settingsService;
        private readonly IFeedService feedService;
        private readonly IUserService userService;

        private UserViewModel currentUser;
        private FeedViewModel currentFeed;
        private SettingsViewModel settings;

        List<(int order, int modelId, View view)> flexLayoutCells = new List<(int, int, View)>();
        private bool hiddingMode = false;
        List<ImageModelAndControl> hidden = new List<ImageModelAndControl>();

        public MainPage(
            IImageInfoService imageService, 
            ISettingsService settingsService, 
            IFeedService feedService,
            IUserService userService)
        {
            this.imageService = imageService;
            this.settingsService = settingsService;
            this.feedService = feedService;
            this.userService = userService;

            currentUser = userService.GetCurrentUser();
            currentFeed = feedService.GetActiveUserFeed(currentUser);
            this.settings = settingsService.GetByFeed(currentFeed);

            InitializeComponent();

            //new TestPictures(service).LoadTestDataToStorage();

            pikFeed.ItemDisplayBinding = new Binding(nameof(FeedViewModel.Name));

            ShowImages();
            UpdateFeedPicker();

            this.SizeChanged += async (s, a) => await ShowImages();
        }

        private void UpdateFeedPicker()
        {
            var allFeeds = feedService.GetByUser(currentUser).OrderBy(x => x.Name).ToList();
            allFeeds.Add(new FeedViewModel { Id = -1, Name = AppResources.AddFeed });           // костыль
            pikFeed.ItemsSource = allFeeds;
            var myCurrentFeed = allFeeds.FirstOrDefault(x => x.Id == (currentFeed?.Id ?? 0));

            pikFeed.SelectedIndexChanged -= FeedSelectedIndexChanged;
            pikFeed.SelectedIndex = pikFeed.ItemsSource.IndexOf(myCurrentFeed);
            pikFeed.SelectedIndexChanged += FeedSelectedIndexChanged;
        }

        private async void FeedSelectedIndexChanged(object sender, EventArgs e)
        {
            FeedViewModel selected = (FeedViewModel)pikFeed.SelectedItem;

            if (selected == null)
                return;

            bool createNew = selected.Id < 0;

            if (createNew)
            {
                // CreateNewFeed
                string name = await DisplayPromptAsync(
                    AppResources.NewFeed,
                    AppResources.InputFeedName,
                    accept: AppResources.Ok,
                    cancel: AppResources.Cancel);

                if (string.IsNullOrEmpty(name))
                    return;

                selected = new FeedViewModel()
                {
                    Name = name,
                    Owner = currentUser,
                    //Settings = settings
                };

                selected.Id = await feedService.Insert(selected);
            }

            currentFeed = selected;
            currentUser.ActiveFeedId = currentFeed.Id;
            await userService.Update(currentUser.Id, currentUser);
            this.settings = settingsService.GetByFeed(currentFeed);
            await ShowImages();

            if (createNew)
                UpdateFeedPicker();
        }

        private async void btnFeedDelete_Clicked(object sender, EventArgs e)
        {
            FeedViewModel selected = (FeedViewModel)pikFeed.SelectedItem;

            if (selected == null || selected.Id < 0)
                return;

            bool approve = await DisplayAlert(
                AppResources.Deleting, 
                AppResources.DeleteThisFeed, 
                AppResources.Delete, 
                AppResources.Cancel);

            if (!approve)
                return;

            foreach (var (_, id, _) in flexLayoutCells)
            {
                await imageService.Delete(id);
            }
            await feedService.Delete(selected.Id);

            currentFeed = feedService.GetActiveUserFeed(currentUser);
            this.settings = settingsService.GetByFeed(currentFeed);
            UpdateFeedPicker();
            await ShowImages();
        }

        private async void btnFeedEdit_Clicked(object sender, EventArgs e)
        {
            FeedViewModel selected = (FeedViewModel)pikFeed.SelectedItem;

            if (selected == null)
                return;

            string name = await DisplayPromptAsync(
                AppResources.Edit,
                AppResources.InputNewName,
                initialValue: selected.Name,
                accept: AppResources.Ok,
                cancel: AppResources.Cancel);

            if (string.IsNullOrEmpty(name))
                return;

            currentFeed.Name = name;
            await feedService.Update(currentFeed.Id, currentFeed);

            UpdateFeedPicker();
        }

        private async void btnSettings_Clicked(object sender, EventArgs e)
        {
            var settingsPage = new SettingsPage(settingsService, settings);
            settingsPage.Disappearing += async (s, a) => 
            { 
                settings = settingsService.GetByFeed(currentFeed);
                await ShowImages();
            };

            await this.Navigation.PushAsync(settingsPage);            
        }

        private async void btnInfo_Clicked(object sender, EventArgs e)
        {
            await this.Navigation.PushAsync(new AboutPage());
        }

        private async void btnAddPhoto_Clicked(object sender, EventArgs e)
        {
            await PickImageAsync();
        }

        private async void btnRefresh_Clicked(object sender, EventArgs e)
        {
            await ShowImages();
        }

        private void btnHidePhoto_Clicked(object sender, EventArgs e)
        {
            if (hiddingMode)
            {
                foreach (var view in hidden)
                {
                    view.Control.IsVisible = true;
                }

                hidden.Clear();
                hiddingMode = false;
                btnHidePhoto.BackgroundColor = Color.LightGray;
                btnHidePhoto.BorderColor = Color.LightGray;
                btnHidePhoto.BorderWidth = 0;
            }
            else
            {
                if (selectedImage != null)
                {
                    HideElement(selectedImage);
                }

                ClearSelection();
                hiddingMode = true;
                btnHidePhoto.BackgroundColor = Color.Gray;
                btnHidePhoto.BorderColor = Color.Red;
                btnHidePhoto.BorderWidth = 2;
            }
        }

        private async void btnRotateRight_Clicked(object sender, EventArgs e)
        {
            if (selectedImage == null)
                return;

            await imageService.RotateImage(selectedImage.ImageInfoId, 90);

            // иначе не придумал как перерисовать картинку
            Image imageControl = FindImageControl(selectedImage.Control);
            if (imageControl != null)
            {
                var src = imageControl.Source;
                imageControl.Source = null;
                imageControl.Source = src;
            }
        }

        private async void btnRemove_Clicked(object sender, EventArgs e)
        {
            if (selectedImage == null)
                return;
            
            bool approve = await DisplayAlert(AppResources.Deleting, AppResources.DeleteThisImage, AppResources.Delete, AppResources.Cancel);

            if (!approve)
                return;

            int deletedId = selectedImage.ImageInfoId;

            await imageService.Delete(deletedId);
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
                    int id = await imageService.InsertFirst(new ImageInfoViewModel()
                    {
                        ImageRef = result.FullPath,
                        SourceType = ImageSourceType.FilePath,
                        Feed = currentFeed
                    });

                    ImageInfoViewModel image = await imageService.Get(id);

                    var stream = await result.OpenReadAsync();
                    AddImageToLayout(image);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert(AppResources.Error, ex.Message, AppResources.Close);
            }
        }

        private async Task ShowImages()
        {
            activityIndicator.IsRunning = true;
            activityIndicator.IsVisible = true;
            pnlEmpty.IsVisible = false;
            ClearSelection();

            try
            {
                flexLayout.Children.Clear();
                flexLayoutCells.Clear();
                List<ImageInfoViewModel> allImages = (await imageService.GetPage(currentFeed, 0, settings.PageSize)).ToList();

                foreach (var image in allImages.OrderBy(x => x.Order))
                {
                    var view = AddImageToLayout(image);

                    if (hiddingMode && hidden.Any(x => x.ImageInfoId == image.Id))
                    {
                        hidden = hidden.Where(x => x.ImageInfoId != image.Id).ToList();
                        HideElement(new ImageModelAndControl(image.Id, view));
                    }
                }

                if (!allImages.Any())
                {
                    pnlEmpty.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                activityIndicator.IsRunning = false;
                activityIndicator.IsVisible = false;
            }
        }

        private View AddImageToLayout(ImageInfoViewModel imageInfo)
        {
            double widthBase = 
                Application.Current?.MainPage?.Width 
                ?? (DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density);
            int imageSizeRequest = (int)(widthBase * (1f / settings.ColumnsCount)) - (settings.ImageSpacingPixels * 2) + 1;

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
            FlexLayout.SetBasis(frame, new FlexBasis(1f / settings.ColumnsCount, true));

            FlexLayout.SetOrder(frame, imageInfo.Order);

            flexLayoutCells.Add((imageInfo.Order, imageInfo.Id, frame));

            pnlEmpty.IsVisible = false;

            return frame;
        }

        private Image FindImageControl(View parent)
        {
            if (parent is Image)
                return parent as Image;

            if (parent is Frame)
                return FindImageControl((parent as Frame).Content);

            if (parent is Layout)
            {
                foreach (var view in (parent as Layout).Children.OfType<Image>())
                {
                    var res = FindImageControl(view);
                    if (res != null)
                        return res;
                }
            }

            return null;
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
            ImageInfoViewModel dragged = await imageService.Get(src.ImageInfoId);
            ImageInfoViewModel landing = await imageService.Get(activeLandingInfo.ImageInfoId);

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
                await imageService.MoveOrder(dragged, endOrder);

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
                VisibleImageOperationButtons(false);
                SetUnselectStyle(selectedImage.Control as Frame);
                selectedImage = null;
            }
        }

        public void SetSelection(ImageModelAndControl selected)
        {
            ClearSelection();

            if (selected == null)
                return;

            if (hiddingMode)
            {
                HideElement(selected);
            }
            else
            {
                Frame frame = selected.Control as Frame;
                SetSelectStyle(frame);
                selectedImage = selected;

                VisibleImageOperationButtons(true);
            }
        }

        private void HideElement(ImageModelAndControl element)
        {
            if (element?.Control == null)
                return;

            element.Control.IsVisible = false;
            hidden.Add(element);
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
            frame.Padding = new Thickness(settings.ImageSpacingPixels);
            frame.Margin = new Thickness(0);
        }

        private void SetUnselectStyle(Frame frame)
        {
            if (frame == null)
                return;

            frame.BorderColor = Color.Default;
            frame.BackgroundColor = Color.Default;
            frame.Margin = new Thickness(settings.ImageSpacingPixels);
            frame.Padding = new Thickness(0);
        }

        private void VisibleImageOperationButtons(bool isVisible)
        {
            btnRemove.IsVisible = isVisible;
            btnRotateRight.IsVisible = isVisible;
        }

        #endregion

    }
}
