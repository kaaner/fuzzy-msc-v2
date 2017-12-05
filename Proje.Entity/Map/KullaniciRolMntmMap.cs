﻿using Proje.Entity.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proje.Entity.Map
{
    public class KullaniciRolMntmMap : EntityTypeConfiguration<KullaniciRolMntm>
    {
        public KullaniciRolMntmMap()
        {
            //Primary Key
            this.HasKey(t => new { t.KullaniciID, t.RolID });

            //Properties
            this.Property(t => t.KullaniciID)
            .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.RolID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.BosKolon)
                .HasMaxLength(1);


            //RelationShips
            this.HasRequired(t => t.Kullanici)
                .WithMany(t => t.KullanicininRolleriMntm)
                .HasForeignKey(t => t.KullaniciID);

            this.HasRequired(t => t.Rol)
                .WithMany(t => t.RolunKullanicilariMntm)
                .HasForeignKey(t => t.RolID);


        }

    }
}
