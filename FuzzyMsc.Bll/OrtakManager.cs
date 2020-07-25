using FuzzyMsc.Bll.Interface;
using FuzzyMsc.Dto.FuzzyDTOS;
using System;
using System.Collections.Generic;

namespace FuzzyMsc.Bll
{
    public class OrtakManager : IOrtakManager
    {
        public List<DegiskenDTO> Mukavemet
        {
            get
            {
                var liste = new List<DegiskenDTO>();
                liste.Add(new DegiskenDTO { Adi = "CokGevsek", GorunenAdi = "Çok Gevşek", MinDeger = 0, MaxDeger = 200 });
                liste.Add(new DegiskenDTO { Adi = "Gevsek", GorunenAdi = "Gevşek", MinDeger = 200, MaxDeger = 300 });
                liste.Add(new DegiskenDTO { Adi = "Orta", GorunenAdi = "Orta", MinDeger = 300, MaxDeger = 500 });
                liste.Add(new DegiskenDTO { Adi = "Siki", GorunenAdi = "Sıkı", MinDeger = 500, MaxDeger = 700 });
                liste.Add(new DegiskenDTO { Adi = "Kaya", GorunenAdi = "Kaya", MinDeger = 700, MaxDeger = Double.MaxValue });
                return liste;
            }
            set { }
        }
        public List<DegiskenDTO> Doygunluk
        {
            get
            {
                var liste = new List<DegiskenDTO>();
                liste.Add(new DegiskenDTO { Adi = "GazaDoygun", GorunenAdi = "Gaza Doygun", MinDeger = 0, MaxDeger = 2 });
                liste.Add(new DegiskenDTO { Adi = "GazaVeSuyaDoygun", GorunenAdi = "Gaza Ve Suya Doygun", MinDeger = 2, MaxDeger = 4 });
                liste.Add(new DegiskenDTO { Adi = "SuyaDoygun", GorunenAdi = "SuyaDoygun", MinDeger = 4, MaxDeger = Double.MaxValue });
                return liste;
            }
            set { }
        }

        public List<ConstantDTO> OperatorList {
            get {
                var liste = new List<ConstantDTO>();
                liste.Add(new ConstantDTO { Text = " and ", Value = 1 });
                liste.Add(new ConstantDTO { Text = " or ", Value = 2 });
                return liste;
            } set { }
        }
        public List<ConstantDTO> MukavemetList {
            get
            {
                var liste = new List<ConstantDTO>();
                liste.Add(new ConstantDTO { Text = "CokGevsek", Value = 1 });
                liste.Add(new ConstantDTO { Text = "Gevsek", Value = 2 });
                liste.Add(new ConstantDTO { Text = "Orta", Value = 3 });
                liste.Add(new ConstantDTO { Text = "Siki", Value = 4 });
                liste.Add(new ConstantDTO { Text = "Kaya", Value = 5 });
                return liste;
            }
            set { }
        }
        public List<ConstantDTO> DoygunlukList {
            get
            {
                var liste = new List<ConstantDTO>();
                liste.Add(new ConstantDTO { Text = "SuyaDoygun", Value = 1 });
                liste.Add(new ConstantDTO { Text = "SuyaVeGazaDoygun", Value = 2 });
                liste.Add(new ConstantDTO { Text = "GazaDoygun", Value = 3 });
                return liste;
            }
            set { }
        }
        public List<ConstantDTO> EsitlikList {
            get
            {
                var liste = new List<ConstantDTO>();
                liste.Add(new ConstantDTO { Text = " is ", Value = 1 });
                liste.Add(new ConstantDTO { Text = " is not ", Value = 2 });
                return liste;
            }
            set { }
        }

        public byte[] StringToByteArray(string value)
        {
            char[] charArr = value.ToCharArray();
            byte[] bytes = new byte[charArr.Length];
            for (int i = 0; i < charArr.Length; i++)
            {
                byte current = Convert.ToByte(charArr[i]);
                bytes[i] = current;
            }

            return bytes;
        }
    }

    public interface IOrtakManager : IBaseManager
    {
        List<DegiskenDTO> Mukavemet { get; set; }
        List<DegiskenDTO> Doygunluk { get; set; }
        
        List<ConstantDTO> OperatorList { get; set; }
        List<ConstantDTO> MukavemetList { get; set; }
        List<ConstantDTO> DoygunlukList { get; set; }
        List<ConstantDTO> EsitlikList { get; set; }

        byte[] StringToByteArray(string value);
    }
}
