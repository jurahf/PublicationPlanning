using PublicationPlanning.StoredModels;
using PublicationPlanning.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using PublicationPlanning.ViewModels;
using PublicationPlanning.Converters;
using System.Threading.Tasks;
using System.Linq;

namespace PublicationPlanning.Services
{
    public interface IImageInfoService : IEntityService<ImageInfoViewModel>
    {
        Task<IEnumerable<ImageInfoViewModel>> GetPage(FeedViewModel feed, int page, int limit);

        Task<int> InsertFirst(ImageInfoViewModel entity);

        Task MoveOrder(ImageInfoViewModel entity, int newOrder);

        Task RotateImage(int entityId, float degrees);
    }

    public class ImageInfoService : BaseEntityService<ImageInfo, ImageInfoViewModel>, IImageInfoService
    {
        private readonly IImageInfoRepository imageRepository;
        private readonly IEntityConverter<Feed, FeedViewModel> feedConverter;

        public ImageInfoService(
            IImageInfoRepository repository,
            IEntityConverter<ImageInfo, ImageInfoViewModel> imageConverter,
            IEntityConverter<Feed, FeedViewModel> feedConverter)
            : base(repository, imageConverter)
        {
            this.imageRepository = repository;
            this.feedConverter = feedConverter;
        }


        public async Task<IEnumerable<ImageInfoViewModel>> GetPage(FeedViewModel feed, int page, int limit)
        {
            Feed dbFeed = feedConverter.ConvertToStoredModel(feed);
            if (dbFeed == null)
            {
                return (await imageRepository.GetPage(page, limit))
                    .Select(x => converter.ConvertToViewModel(x));
            }
            else
            {
                return (await imageRepository.GetPage(dbFeed, page, limit))
                    .Select(x => converter.ConvertToViewModel(x));
            }
        }


        public async Task<int> InsertFirst(ImageInfoViewModel entity)
        {
            ImageInfo oldFirst = (await imageRepository.GetPage(0, 1)).FirstOrDefault();
            entity.Order = oldFirst == null ? 0 : oldFirst.Order - 1;

            return await base.Insert(entity);
        }

        public async Task MoveOrder(ImageInfoViewModel entity, int newOrder)
        {
            if (entity == null)
                return;

            int oldOrder = entity.Order;

            List<ImageInfo> imageList = 
                (await imageRepository.GetByOrders(Math.Min(oldOrder, newOrder), Math.Max(oldOrder, newOrder)))
                .ToList();

            int moveDirection = oldOrder < newOrder ? -1 : 1;
            foreach (var image in imageList)
            {
                image.Order += moveDirection;
            }

            ImageInfo moved = imageList.FirstOrDefault(x => x.Id == entity.Id);
            if (moved != null)
                moved.Order = newOrder;

            foreach (var image in imageList)
            {
                await imageRepository.Update(image.Id, image);
            }
        }

        public async Task RotateImage(int entityId, float degrees)
        {
            ImageInfo entity = await imageRepository.Get(entityId);

            await imageRepository.RotateImage(entity, degrees);
        }

    }
}
