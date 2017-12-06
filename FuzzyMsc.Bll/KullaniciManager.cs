using FuzzyMsc.Dto;
using FuzzyMsc.Service;
using FuzzyMsc.Bll.Interface;
using FuzzyMsc.Pattern.UnitOfWork;
using System;
using System.Linq;

namespace FuzzyMsc.Bll
{
    public interface IKullaniciManager : IBaseManager
    {
        SonucDTO Kaydet(int parametre1, string parametre2);

        SonucDTO Getir();
    }

    public class KullaniciManager : IKullaniciManager
    {
        IUnitOfWorkAsync _unitOfWork;
        IKullaniciService _kullaniciService;

        public KullaniciManager(
            IUnitOfWorkAsync unitOfWork,
            IKullaniciService kullaniciService)
        {
            _unitOfWork = unitOfWork;
            _kullaniciService = kullaniciService;
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
        public SonucDTO Kaydet(int parametre1,string parametre2)
        {
            //Metodumuz try catch içinde olmalı ve tüm metodlar SonucDTO tipinde nesne döndürmeli 
            SonucDTO sonuc = new SonucDTO();
            try
            {
                //işlemler yapılıyor...
                sonuc.Mesaj = "Kaydetme başarılı";
                sonuc.Nesne = null;//Get işlemlerinde nesne buraya eklenecek.
                sonuc.Sonuc = true;                
            }
            catch (Exception ex)
            {
                sonuc.Mesaj = "Kaydetme başarısız";
                sonuc.Sonuc = false;
                sonuc.Exception = ex;           
            }
            return sonuc;
        }


        public SonucDTO Getir()
        {
            SonucDTO sonuc = new SonucDTO();
            try
            {
                //işlemler yapılıyor...
                sonuc.Mesaj = "İşlem Başarılı";
                sonuc.Nesne = _kullaniciService.Queryable().FirstOrDefault().Ad;
                sonuc.Sonuc = true;
            }
            catch (Exception ex)
            {
                sonuc.Mesaj = "Kaydetme başarısız";
                sonuc.Sonuc = false;
                sonuc.Exception = ex;
            }
            return sonuc;
           
        }

        #endregion

    }
}
