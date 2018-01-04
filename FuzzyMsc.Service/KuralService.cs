using FuzzyMsc.Service.Interface;
using FuzzyMsc.Entity.Model;
using FuzzyMsc.Pattern.Repositories;
using FuzzyMsc.ServicePattern;

namespace FuzzyMsc.Service
{
    public class KuralService : Service<Kural>, IKuralService
    {
        private readonly IRepositoryAsync<Kural> _repository;
        public KuralService(IRepositoryAsync<Kural> repository) : base(repository)
        {
            _repository = repository;
        }
    }

    public interface IKuralService : IService<Kural>, IBaseService
    {

    }
}