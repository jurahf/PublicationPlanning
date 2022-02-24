using PublicationPlanning.StoredModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PublicationPlanning.Repositories
{
    public interface IFeedRepository : IRepository<Feed>
    {
        List<Feed> GetByUser(User user);

        Feed GetActiveUserFeed(User user);
    }


    public class FeedFileRepository : BaseFileRepository<Feed>, IFeedRepository
    {
        public List<Feed> GetByUser(User user)
        {
            return allData.Where(x => x.Owner?.Id == user.Id).ToList();
        }

        public Feed GetActiveUserFeed(User user)
        {
            Feed result = allData.FirstOrDefault(x => x.Owner?.Id == user.Id && x.Id == user.ActiveFeedId);
            
            if (result == null)
            {
                result = allData.FirstOrDefault(x => x.Owner?.Id == user.Id);
                if (result != null)
                    user.ActiveFeedId = result.Id;
            }

            if (result == null)
            {
                result = GetDefaultFeed(user);
                allData.Add(result);
                user.ActiveFeedId = result.Id;
                Update(result.Id, result);
            }

            return result;
        }

        private Feed GetDefaultFeed(User user)
        {
            return new Feed()
            {
                Id = GetNewId(),
                Owner = user,
                Name = AppResources.DefaultFeedName,
            };
        }
    }
}
