using Proje.BL.Interface;
using Proje.DTO;
using Proje.Entity;
using Proje.Entity.Model;
using Proje.Pattern.DataContext;
using Proje.Pattern.EF6;
using Proje.Pattern.Repositories;
using Proje.Pattern.UnitOfWork;
using Proje.Service;
using Proje.ServicePattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Proje.BL
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
