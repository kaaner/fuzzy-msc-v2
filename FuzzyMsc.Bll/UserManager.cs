using FuzzyMsc.Dto;
using FuzzyMsc.Service;
using FuzzyMsc.Bll.Interface;
using FuzzyMsc.Pattern.UnitOfWork;
using System;
using System.Linq;

namespace FuzzyMsc.Bll
{
    public interface IUserManager : IBaseManager
    {
        ResultDTO Save(int parametre1, string parametre2);

        ResultDTO Get();
    }

    public class UserManager : IUserManager
    {
        IUnitOfWorkAsync _unitOfWork;
        IUserService _userService;

        public UserManager(
            IUnitOfWorkAsync unitOfWork,
            IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
        }

        #region Getirme işlemleri
        //bu şekilde region kullanarak benzer yerden istek yapılan işlemleri bir arada toplayabiliriz.
        //Örnek olarak kullanici getir, kullanici sil, kullanici güncelle bir arada toplanabilir.

        /// <summary>
        /// Buası metodumuzun açıklama satırı metodu yazdıktan sonra yukarısına gelip /// yazınca otomatik geliyor.
        /// burada metodumuzun ne iş yaptığını tanımlayacağız. Diğer kullanıcılar anlayabilsin diye
        /// </summary>
        /// <param name="parametre1">parametre 1'in açıklaması</param>
        /// <param name="parametre2">parametre 2'nin açıklaması</param>
        public ResultDTO Save(int parametre1,string parametre2)
        {
            //Metodumuz try catch içinde olmalı ve tüm metodlar SonucDTO tipinde nesne döndürmeli 
            ResultDTO sonuc = new ResultDTO();
            try
            {
                //işlemler yapılıyor...
                sonuc.Message = "Kaydetme başarılı";
                sonuc.Object = null;//Get işlemlerinde nesne buraya eklenecek.
                sonuc.Result = true;                
            }
            catch (Exception ex)
            {
                sonuc.Message = "Kaydetme başarısız";
                sonuc.Result = false;
                sonuc.Exception = ex;           
            }
            return sonuc;
        }


        public ResultDTO Get()
        {
            ResultDTO sonuc = new ResultDTO();
            try
            {
                //işlemler yapılıyor...
                sonuc.Message = "İşlem Başarılı";
                sonuc.Object = _userService.Queryable().FirstOrDefault().Adi;
                sonuc.Result = true;
            }
            catch (Exception ex)
            {
                sonuc.Message = "Kaydetme başarısız";
                sonuc.Result = false;
                sonuc.Exception = ex;
            }
            return sonuc;
           
        }

        #endregion

    }
}
