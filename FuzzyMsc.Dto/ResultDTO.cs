using System;

namespace FuzzyMsc.Dto
{
    public class ResultDTO
    {

        /// <summary>
        /// Message that to be shown to the user in the interface (Arayüzde kullanıcıya gösterilecek mesaj)
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Result of the operation (İşlemin başarılı olup olmadığı)
        /// </summary>
        public bool Result { get; set; }

        /// <summary>
        /// Return object (Döndürülecek Nesne)
        /// </summary>
        public object Object { get; set; }

        /// <summary>
        /// Exception
        /// </summary>
        public Exception Exception { get; set; }
    }
}
