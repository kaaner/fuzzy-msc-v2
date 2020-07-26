using FuzzyMsc.Service.Interface;
using FuzzyMsc.Entity.Model;
using FuzzyMsc.Pattern.Repositories;
using FuzzyMsc.ServicePattern;

namespace FuzzyMsc.Service
{
    public class RuleService : Service<Kural>, IRuleService
    {
        private readonly IRepositoryAsync<Kural> _repository;
        public RuleService(IRepositoryAsync<Kural> repository) : base(repository)
        {
            _repository = repository;
        }
    }

    public interface IRuleService : IService<Kural>, IBaseService
    {

    }
}