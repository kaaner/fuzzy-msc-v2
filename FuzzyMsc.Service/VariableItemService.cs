using FuzzyMsc.Service.Interface;
using FuzzyMsc.Entity.Model;
using FuzzyMsc.Pattern.Repositories;
using FuzzyMsc.ServicePattern;

namespace FuzzyMsc.Service
{
    public class VariableItemService : Service<DegiskenItem>, IVariableItemService
    {
        private readonly IRepositoryAsync<DegiskenItem> _repository;
        public VariableItemService(IRepositoryAsync<DegiskenItem> repository) : base(repository)
        {
            _repository = repository;
        }
    }

    public interface IVariableItemService : IService<DegiskenItem>, IBaseService
    {

    }
}
