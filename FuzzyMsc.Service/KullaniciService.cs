using FuzzyMsc.Service.Interface;
using FuzzyMsc.Entity.Model;
using FuzzyMsc.Pattern.Repositories;
using FuzzyMsc.ServicePattern;

namespace FuzzyMsc.Service
{
    public class KullaniciService : Service<Kullanici>, IKullaniciService
    {
        private readonly IRepositoryAsync<Kullanici> _repository;
        public KullaniciService(IRepositoryAsync<Kullanici> repository) : base(repository)
        {
            _repository = repository;
        }
    }

    public interface IKullaniciService : IService<Kullanici>, IBaseService
    {

    }
}
