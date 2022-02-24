using PublicationPlanning.Converters;
using PublicationPlanning.Repositories;
using PublicationPlanning.StoredModels;
using PublicationPlanning.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PublicationPlanning.Services
{
    public interface IFeedService : IEntityService<FeedViewModel>
    {
        List<FeedViewModel> GetByUser(UserViewModel user);

        FeedViewModel GetActiveUserFeed(UserViewModel user);
    }


    public class FeedService : BaseEntityService<Feed, FeedViewModel>, IFeedService
    {
        private readonly IFeedRepository feedRepository;
        private readonly IEntityConverter<User, UserViewModel> userConverter;

        public FeedService(
            IFeedRepository repository, 
            IEntityConverter<Feed, FeedViewModel> feedConverter,
            IEntityConverter<User, UserViewModel> userConverter)
            : base(repository, feedConverter)
        {
            this.feedRepository = repository;
            this.userConverter = userConverter;
        }

        public FeedViewModel GetActiveUserFeed(UserViewModel user)
        {
            User dbUser = userConverter.ConvertToStoredModel(user);
            Feed feed = feedRepository.GetActiveUserFeed(dbUser);
            return converter.ConvertToViewModel(feed);
        }

        public List<FeedViewModel> GetByUser(UserViewModel user)
        {
            User dbUser = userConverter.ConvertToStoredModel(user);
            List<Feed> feeds = feedRepository.GetByUser(dbUser);
            return feeds
                .Select(x => converter.ConvertToViewModel(x))
                .ToList();
        }
    }
}
