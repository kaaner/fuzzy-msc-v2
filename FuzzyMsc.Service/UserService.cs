using FuzzyMsc.Service.Interface;
using FuzzyMsc.Entity.Model;
using FuzzyMsc.Pattern.Repositories;
using FuzzyMsc.ServicePattern;

namespace FuzzyMsc.Service
{
    public class UserService : Service<Kullanici>, IUserService
    {
        private readonly IRepositoryAsync<Kullanici> _repository;
        public UserService(IRepositoryAsync<Kullanici> repository) : base(repository)
        {
            _repository = repository;
        }
    }

    public interface IUserService : IService<Kullanici>, IBaseService
    {

    }
}
