using System;

namespace FuzzyMsc.DTO
{
    public class SonucDTO
    {

        /// <summary>
        /// Arayüzde kullanıcıya gösterilecek mesaj
        /// </summary>
        public string Mesaj { get; set; }

        /// <summary>
        /// İşlemin başarılı olup olmadığı
        /// </summary>
        public bool Sonuc { get; set; }

        /// <summary>
        /// Döndürülecek Nesne
        /// </summary>
        public object Nesne { get; set; }

        /// <summary>
        /// Metod catch'e düştüğünde oluşan exception
        /// </summary>
        public Exception Exception { get; set; }
    }
}
