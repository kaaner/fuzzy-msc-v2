using Proje.Entity.Model;
using Proje.Pattern.Repositories;
using Proje.Service.Interface;
using Proje.ServicePattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proje.Service
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
