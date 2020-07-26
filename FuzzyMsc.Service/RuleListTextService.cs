using FuzzyMsc.Service.Interface;
using FuzzyMsc.Entity.Model;
using FuzzyMsc.Pattern.Repositories;
using FuzzyMsc.ServicePattern;

namespace FuzzyMsc.Service
{
    public class RuleListTextService : Service<KuralListText>, IRuleListTextService
    {
        private readonly IRepositoryAsync<KuralListText> _repository;
        public RuleListTextService(IRepositoryAsync<KuralListText> repository) : base(repository)
        {
            _repository = repository;
        }
    }

    public interface IRuleListTextService : IService<KuralListText>, IBaseService
    {

    }
}
