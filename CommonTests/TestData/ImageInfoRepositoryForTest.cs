using PublicationPlanning.Repositories;
using PublicationPlanning.StoredModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace CommonTests.TestData
{
    internal class ImageInfoRepositoryForTest : IImageInfoRepository
    {
        private List<ImageInfo> allData = new List<ImageInfo>();

        public ImageInfoRepositoryForTest()
        {
            allData = new List<ImageInfo>()
            {
                new ImageInfo()
                {
                    Id = 1,
                    Order = 1,
                    SourceType = ImageSourceType.FilePath,
                },
                new ImageInfo()
                {
                    Id = 2,
                    Order = 2,
                    SourceType = ImageSourceType.FilePath,
                },
                new ImageInfo()
                {
                    Id = 3,
                    Order = 3,
                    SourceType = ImageSourceType.FilePath,
                },
                new ImageInfo()
                {
                    Id = 5,
                    Order = 5,
                    SourceType = ImageSourceType.FilePath,
                },
            };
        }


        public Task<bool> Delete(int id)
        {
            throw new NotImplementedException();
        }

        public Task<ImageInfo> Get(int id)
        {
            return Task.FromResult(allData.FirstOrDefault(x => x.Id == id));
        }

        public Task<IEnumerable<ImageInfo>> GetByOrders(int startOrder, int endOrder)
        {
            return Task.FromResult(
                allData.Where(x => x.Order >= startOrder && x.Order <= endOrder)
                );
        }

        public Task<IEnumerable<ImageInfo>> GetPage(Feed feed, int page, int limit)
        {
            return Task.FromResult(
                allData
                    .Where(x => x.Feed?.Id == feed.Id)
                    .OrderBy(x => x.DefaultOrder())
                    .Take(limit)
                );
        }

        public Task<IEnumerable<ImageInfo>> GetPage(int page, int limit)
        {
            return Task.FromResult(
                allData
                    .OrderBy(x => x.DefaultOrder())
                    .Take(limit)
                );
        }

        public Task<int> Insert(ImageInfo entity)
        {
            int id = allData.Max(x => x.Id) + 1;
            entity.Id = id;
            allData.Add(entity);
            return Task.FromResult(id);
        }

        public Task RotateImage(ImageInfo imageInfo, float degrees)
        {
            throw new NotImplementedException();
        }

        public Task<int> Update(int id, ImageInfo entity)
        {
            allData = allData.Where(x => x.Id != id).ToList();
            allData.Add(entity);
            return Task.FromResult(entity.Id);
        }
    }
}
