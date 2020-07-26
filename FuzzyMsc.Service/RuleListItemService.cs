using FuzzyMsc.Service.Interface;
using FuzzyMsc.Entity.Model;
using FuzzyMsc.Pattern.Repositories;
using FuzzyMsc.ServicePattern;

namespace FuzzyMsc.Service
{
    public class RuleListItemService : Service<KuralListItem>, IRuleListItemService
    {
        private readonly IRepositoryAsync<KuralListItem> _repository;
        public RuleListItemService(IRepositoryAsync<KuralListItem> repository) : base(repository)
        {
            _repository = repository;
        }
    }

    public interface IRuleListItemService : IService<KuralListItem>, IBaseService
    {

    }
}
