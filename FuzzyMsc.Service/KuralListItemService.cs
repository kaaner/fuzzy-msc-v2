using FuzzyMsc.Service.Interface;
using FuzzyMsc.Entity.Model;
using FuzzyMsc.Pattern.Repositories;
using FuzzyMsc.ServicePattern;

namespace FuzzyMsc.Service
{
    public class KuralListItemService : Service<KuralListItem>, IKuralListItemService
    {
        private readonly IRepositoryAsync<KuralListItem> _repository;
        public KuralListItemService(IRepositoryAsync<KuralListItem> repository) : base(repository)
        {
            _repository = repository;
        }
    }

    public interface IKuralListItemService : IService<KuralListItem>, IBaseService
    {

    }
}
