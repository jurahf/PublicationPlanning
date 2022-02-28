using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Xunit;
using Moq;
using PublicationPlanning.Services;
using PublicationPlanning.Repositories;
using PublicationPlanning.StoredModels;
using PublicationPlanning.Converters;
using CommonTests.TestData;
using PublicationPlanning.ViewModels;

namespace CommonTests
{
    public class ImageInfoServiceTest
    {
        private readonly ImageInfoService service;

        public ImageInfoServiceTest()
        {
            UserConverter userConverter = new UserConverter();
            SettingsConverter settingsConverter = new SettingsConverter();
            FeedConverter feedConverter = new FeedConverter(userConverter, settingsConverter);
            ImageInfoConverter imageConverter = new ImageInfoConverter(feedConverter);

            service = new ImageInfoService(
                new ImageInfoRepositoryForTest(),
                imageConverter,
                feedConverter);
        }

        [Fact]
        public async void MoveOrder_Empty()
        {
            await service.MoveOrder(null, 0);

            List<ImageInfoViewModel> result = (await service.GetPage(0, 100)).ToList();
            Assert.Equal(4, result.Count);
            foreach (var item in result.OrderBy(x => x.Order))
            {
                Assert.Equal(item.Order, item.Id);  // ничего не поменялось
            }
        }

        /// начальный порядок - 1, 2, 3, 5
        [Theory]
        [InlineData(1, 1, new int[] { 1, 2, 3, 5 })]
        [InlineData(1, 3, new int[] { 3, 1, 2, 5 })]
        [InlineData(5, 1, new int[] { 2, 3, 4, 1 })]
        [InlineData(1, 4, new int[] { 4, 1, 2, 5 })]
        public async void MoveOrder(int srcId, int newOrder, int[] expectedOrders)
        {
            ImageInfoViewModel info = await service.Get(srcId);

            await service.MoveOrder(info, newOrder);

            List<ImageInfoViewModel> result = (await service.GetPage(0, 100)).ToList();
            Assert.Equal(4, result.Count);

            Assert.Equal(expectedOrders[0], result.First(x => x.Id == 1).Order);
            Assert.Equal(expectedOrders[1], result.First(x => x.Id == 2).Order);
            Assert.Equal(expectedOrders[2], result.First(x => x.Id == 3).Order);
            Assert.Equal(expectedOrders[3], result.First(x => x.Id == 5).Order);
        }

        [Fact]
        public async void InsertFirst()
        {
            int id1 = await service.InsertFirst(
                new ImageInfoViewModel() { SourceType = ImageSourceType.FilePath });
            int id2 = await service.InsertFirst(
                new ImageInfoViewModel() { Order = 1000, SourceType = ImageSourceType.FilePath });

            List<ImageInfoViewModel> result = (await service.GetPage(0, 100)).ToList();
            Assert.Equal(6, result.Count);

            Assert.Equal(1, result.First(x => x.Id == 1).Order);
            Assert.Equal(2, result.First(x => x.Id == 2).Order);
            Assert.Equal(3, result.First(x => x.Id == 3).Order);
            Assert.Equal(5, result.First(x => x.Id == 5).Order);
            Assert.Equal(0, result.First(x => x.Id == id1).Order);
            Assert.Equal(-1, result.First(x => x.Id == id2).Order);
        }

    }
}
