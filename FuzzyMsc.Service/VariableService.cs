using FuzzyMsc.Service.Interface;
using FuzzyMsc.Entity.Model;
using FuzzyMsc.Pattern.Repositories;
using FuzzyMsc.ServicePattern;

namespace FuzzyMsc.Service
{
    public class VariableService : Service<Degisken>, IVariableService
    {
        private readonly IRepositoryAsync<Degisken> _repository;
        public VariableService(IRepositoryAsync<Degisken> repository) : base(repository)
        {
            _repository = repository;
        }
    }

    public interface IVariableService : IService<Degisken>, IBaseService
    {

    }
}