using FuzzyMsc.Service.Interface;
using FuzzyMsc.Entity.Model;
using FuzzyMsc.Pattern.Repositories;
using FuzzyMsc.ServicePattern;

namespace FuzzyMsc.Service
{
    public class KuralListService : Service<KuralList>, IKuralListService
    {
        private readonly IRepositoryAsync<KuralList> _repository;
        public KuralListService(IRepositoryAsync<KuralList> repository) : base(repository)
        {
            _repository = repository;
        }
    }

    public interface IKuralListService : IService<KuralList>, IBaseService
    {

    }
}
