using PublicationPlanning.Converters;
using PublicationPlanning.Repositories;
using PublicationPlanning.StoredModels;
using PublicationPlanning.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace PublicationPlanning.Services
{
    public interface IUserService : IEntityService<UserViewModel>
    {
        UserViewModel GetCurrentUser();
    }

    public class UserService : BaseEntityService<User, UserViewModel>, IUserService
    {
        private readonly IUserRepository userRepository;

        public UserService(IUserRepository repository, IEntityConverter<User, UserViewModel> converter)
            : base(repository, converter)
        {
            this.userRepository = repository;
        }

        public UserViewModel GetCurrentUser()
        {
            return converter.ConvertToViewModel(userRepository.GetCurrentUser());
        }

    }
}
