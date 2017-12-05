using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proje.Entity.Model
{
    public partial class KullaniciRolMntm
    {
        public long KullaniciID { get; set; }

        public int RolID { get; set; }

        public string BosKolon { get; set; }





        //Virtual
        public virtual Kullanici Kullanici { get; set; }

        public virtual Rol Rol { get; set; }

    }
}
