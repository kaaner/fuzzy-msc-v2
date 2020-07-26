using FuzzyMsc.Service.Interface;
using FuzzyMsc.Entity.Model;
using FuzzyMsc.Pattern.Repositories;
using FuzzyMsc.ServicePattern;

namespace FuzzyMsc.Service
{
    public class RuleListService : Service<KuralList>, IRuleListService
    {
        private readonly IRepositoryAsync<KuralList> _repository;
        public RuleListService(IRepositoryAsync<KuralList> repository) : base(repository)
        {
            _repository = repository;
        }
    }

    public interface IRuleListService : IService<KuralList>, IBaseService
    {

    }
}
