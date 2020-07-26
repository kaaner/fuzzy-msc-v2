using FuzzyMsc.Bll.Interface;
using FuzzyMsc.Dto.FuzzyDTOS;
using System;
using System.Collections.Generic;

namespace FuzzyMsc.Bll
{
    public class CommonManager : ICommonManager
    {
        public List<VariableDTO> Mukavemet
        {
            get
            {
                var liste = new List<VariableDTO>();
                liste.Add(new VariableDTO { Name = "CokGevsek", VisibleName = "Çok Gevşek", MinValue = 0, MaxValue = 200 });
                liste.Add(new VariableDTO { Name = "Gevsek", VisibleName = "Gevşek", MinValue = 200, MaxValue = 300 });
                liste.Add(new VariableDTO { Name = "Orta", VisibleName = "Orta", MinValue = 300, MaxValue = 500 });
                liste.Add(new VariableDTO { Name = "Siki", VisibleName = "Sıkı", MinValue = 500, MaxValue = 700 });
                liste.Add(new VariableDTO { Name = "Kaya", VisibleName = "Kaya", MinValue = 700, MaxValue = Double.MaxValue });
                return liste;
            }
            set { }
        }
        public List<VariableDTO> Doygunluk
        {
            get
            {
                var liste = new List<VariableDTO>();
                liste.Add(new VariableDTO { Name = "GazaDoygun", VisibleName = "Gaza Doygun", MinValue = 0, MaxValue = 2 });
                liste.Add(new VariableDTO { Name = "GazaVeSuyaDoygun", VisibleName = "Gaza Ve Suya Doygun", MinValue = 2, MaxValue = 4 });
                liste.Add(new VariableDTO { Name = "SuyaDoygun", VisibleName = "SuyaDoygun", MinValue = 4, MaxValue = Double.MaxValue });
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

    public interface ICommonManager : IBaseManager
    {
        List<VariableDTO> Mukavemet { get; set; }
        List<VariableDTO> Doygunluk { get; set; }
        
        List<ConstantDTO> OperatorList { get; set; }
        List<ConstantDTO> MukavemetList { get; set; }
        List<ConstantDTO> DoygunlukList { get; set; }
        List<ConstantDTO> EsitlikList { get; set; }

        byte[] StringToByteArray(string value);
    }
}
