using FuzzyMsc.Service.Interface;
using FuzzyMsc.Entity.Model;
using FuzzyMsc.Pattern.Repositories;
using FuzzyMsc.ServicePattern;

namespace FuzzyMsc.Service
{
    public class DegiskenService : Service<Degisken>, IDegiskenService
    {
        private readonly IRepositoryAsync<Degisken> _repository;
        public DegiskenService(IRepositoryAsync<Degisken> repository) : base(repository)
        {
            _repository = repository;
        }
    }

    public interface IDegiskenService : IService<Degisken>, IBaseService
    {

    }
}