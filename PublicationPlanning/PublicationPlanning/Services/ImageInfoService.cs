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
        Task<int> InsertFirst(ImageInfoViewModel entity);

        Task MoveOrder(ImageInfoViewModel entity, int newOrder);
    }

    public class ImageInfoService : BaseEntityService<ImageInfo, ImageInfoViewModel>, IImageInfoService
    {
        private readonly IImageInfoRepository imageRepository;

        public ImageInfoService(
            IImageInfoRepository repository,
            IEntityConverter<ImageInfo, ImageInfoViewModel> converter)
            : base(repository, converter)
        {
            this.imageRepository = repository;
        }

        public async Task<int> InsertFirst(ImageInfoViewModel entity)
        {
            ImageInfo oldFirst = (await repository.GetPage(0, 1)).FirstOrDefault();
            entity.Order = oldFirst == null ? 0 : oldFirst.Order - 1;

            return await base.Insert(entity);
        }

        public async Task MoveOrder(ImageInfoViewModel entity, int newOrder)
        {
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
                await repository.Update(image.Id, image);
            }
        }
    }
}
