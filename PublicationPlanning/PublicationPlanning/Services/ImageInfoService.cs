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

        //void MoveOrder(ImageInfo entity, int newOrder);
    }

    public class ImageInfoService : BaseEntityService<ImageInfo, ImageInfoViewModel>, IImageInfoService
    {
        private readonly IImageInfoRepository repository;

        public ImageInfoService(
            IImageInfoRepository repository,
            IEntityConverter<ImageInfo, ImageInfoViewModel> converter)
            : base(repository, converter)
        {
            this.repository = repository;
        }

        public async Task<int> InsertFirst(ImageInfoViewModel entity)
        {
            ImageInfo oldFirst = (await repository.GetPage(0, 1)).FirstOrDefault();
            entity.Order = oldFirst == null ? 0 : oldFirst.Order - 1;

            return await base.Insert(entity);
        }
    }
}
