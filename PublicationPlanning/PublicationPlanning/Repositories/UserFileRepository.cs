using PublicationPlanning.StoredModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PublicationPlanning.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        User GetCurrentUser();
    }

    public class UserFileRepository : BaseFileRepository<User>, IUserRepository
    {
        public User GetCurrentUser()
        {
            return allData.FirstOrDefault() ?? GetDefaultUser();
        }

        private User GetDefaultUser()
        {
            return new User()
            {
                Id = 1
            };
        }
    }
}
