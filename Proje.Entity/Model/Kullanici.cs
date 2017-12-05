using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proje.Entity.Model
{
    public partial class Kullanici
    {

        public Kullanici()
        {
            this.KullanicininRolleriMntm = new List<KullaniciRolMntm>();
        }

        public long KullaniciID { get; set; }

        public string Guid { get; set; }

        public string Ad { get; set; }

        public string Soyad { get; set; }

        public string TcNo { get; set; }

        public string Sifre { get; set; }

        public string Eposta { get; set; }

        public string Telefon { get; set; }


        //Virtual
        public ICollection<KullaniciRolMntm> KullanicininRolleriMntm { get; set; }


    }
}
