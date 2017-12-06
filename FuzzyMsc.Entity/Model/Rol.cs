using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuzzyMsc.Entity.Model
{
    public partial class Rol
    {

        public Rol()
        {
            this.RolunKullanicilariMntm = new List<KullaniciRolMntm>();
        }

        public int RolID { get; set; }

        public string RolAdi { get; set; }


        //Virtaul

        public ICollection<KullaniciRolMntm> RolunKullanicilariMntm { get; set; }
    }
}
