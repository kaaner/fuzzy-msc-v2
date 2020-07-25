using FuzzyMsc.Service.Interface;
using FuzzyMsc.Entity.Model;
using FuzzyMsc.Pattern.Repositories;
using FuzzyMsc.ServicePattern;

namespace FuzzyMsc.Service
{
    public class KuralListTextService : Service<KuralListText>, IKuralListTextService
    {
        private readonly IRepositoryAsync<KuralListText> _repository;
        public KuralListTextService(IRepositoryAsync<KuralListText> repository) : base(repository)
        {
            _repository = repository;
        }
    }

    public interface IKuralListTextService : IService<KuralListText>, IBaseService
    {

    }
}
