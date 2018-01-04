using FuzzyMsc.Service.Interface;
using FuzzyMsc.Entity.Model;
using FuzzyMsc.Pattern.Repositories;
using FuzzyMsc.ServicePattern;

namespace FuzzyMsc.Service
{
    public class DegiskenItemService : Service<DegiskenItem>, IDegiskenItemService
    {
        private readonly IRepositoryAsync<DegiskenItem> _repository;
        public DegiskenItemService(IRepositoryAsync<DegiskenItem> repository) : base(repository)
        {
            _repository = repository;
        }
    }

    public interface IDegiskenItemService : IService<DegiskenItem>, IBaseService
    {

    }
}
