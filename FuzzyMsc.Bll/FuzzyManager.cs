using FuzzyMsc.Bll.Interface;
using FuzzyMsc.Core.Enums;
using FuzzyMsc.Dto.FuzzyDTOS;
using FuzzyMsc.FuzzyLibrary;
using FuzzyMsc.Pattern.UnitOfWork;
using FuzzyMsc.Service;
using System.Collections.Generic;

namespace FuzzyMsc.Bll
{
    public class FuzzyManager : IFuzzyManager
    {

        IUnitOfWorkAsync _unitOfWork;
        IOrtakManager _ortakManager;
        IKullaniciService _kullaniciService;

        public FuzzyManager(
            IUnitOfWorkAsync unitOfWork,
            IKullaniciService kullaniciService,
            IOrtakManager ortakManager)
        {
            _unitOfWork = unitOfWork;
            _ortakManager = ortakManager;
            _kullaniciService = kullaniciService;
        }

        public void KurallariOlusturFLL(KuralKumeDTO kuralKume)
        {
            var ozdirenc = GorunenAdDuzenle(kuralKume.OzdirencList);
            var toprak = GorunenAdDuzenle(kuralKume.ToprakList);

            MamdaniFuzzySystem fsToprak = new MamdaniFuzzySystem();

            #region Inputs
            FuzzyVariable fvOzdirenc = new FuzzyVariable("Ozdirenc", 0.0, 1000.0);
            foreach (var item in ozdirenc)
            {
                fvOzdirenc.Terms.Add(new FuzzyTerm(item.Adi, new TriangularMembershipFunction(item.MinDeger, (item.MinDeger + item.MaxDeger) / 2, item.MaxDeger)));
            }
            fsToprak.Input.Add(fvOzdirenc);

            FuzzyVariable fvMukavemet = new FuzzyVariable("Mukavemet", 0.0, 1000.0);
            foreach (var item in _ortakManager.Mukavemet)
            {
                fvMukavemet.Terms.Add(new FuzzyTerm(item.Adi, new TriangularMembershipFunction(item.MinDeger, (item.MinDeger + item.MaxDeger) / 2, item.MaxDeger)));
            }
            fsToprak.Input.Add(fvMukavemet);

            FuzzyVariable fvDoygunluk = new FuzzyVariable("Doygunluk", 0.0, 10.0);
            foreach (var item in _ortakManager.Doygunluk)
            {
                fvDoygunluk.Terms.Add(new FuzzyTerm(item.Adi, new TriangularMembershipFunction(item.MinDeger, (item.MinDeger + item.MaxDeger) / 2, item.MaxDeger)));
            }
            fsToprak.Input.Add(fvDoygunluk);
            #endregion

            #region Output
            FuzzyVariable fvToprak = new FuzzyVariable("Toprak", 0.0, 1000.0);
            foreach (var item in toprak)
            {
                fvToprak.Terms.Add(new FuzzyTerm(item.Adi, new TriangularMembershipFunction(item.MinDeger, (item.MinDeger + item.MaxDeger) / 2, item.MaxDeger)));
            }
            fsToprak.Output.Add(fvToprak);
            #endregion
            List<string> kurallar = new List<string>();

            foreach (var KuralListItem in kuralKume.KuralList)
            {
                string ruleText = KuralOlustur(KuralListItem) + " then (Toprak is " + KuralListItem.Sonuc + ")";
                kurallar.Add(ruleText);
            }



            throw new System.NotImplementedException();
        }

        public void KurallariOlusturFLS(KuralKumeDTO kuralKume)
        {
            //var ozdirenc = GorunenAdDuzenle(kuralKume.OzdirencList);
            //#region Inputs
            //List<LinguisticVariable> inputs = new List<LinguisticVariable>();

            //var Ozdirenc = new LinguisticVariable("Ozdirenc");
            //List<IMembershipFunction> InputMembershipFunctions = new List<IMembershipFunction>();
            //foreach (var item in ozdirenc)
            //{
            //    InputMembershipFunctions.Add(Ozdirenc.MembershipFunctions.AddRectangle(item.Adi, item.MinDeger, item.MaxDeger));
            //}
            //inputs.Add(Ozdirenc);
            //var Mukavemet = new LinguisticVariable("Mukavemet");
            //var CokGevsek = Mukavemet.MembershipFunctions.AddRectangle(_ortakManager.Mukavemet[0].Adi, _ortakManager.Mukavemet[0].MinDeger, _ortakManager.Mukavemet[0].MaxDeger);
            //var Gevsek = Mukavemet.MembershipFunctions.AddRectangle(_ortakManager.Mukavemet[1].Adi, _ortakManager.Mukavemet[1].MinDeger, _ortakManager.Mukavemet[1].MaxDeger);
            //var Orta = Mukavemet.MembershipFunctions.AddRectangle(_ortakManager.Mukavemet[2].Adi, _ortakManager.Mukavemet[2].MinDeger, _ortakManager.Mukavemet[2].MaxDeger);
            //var Siki = Mukavemet.MembershipFunctions.AddRectangle(_ortakManager.Mukavemet[3].Adi, _ortakManager.Mukavemet[3].MinDeger, _ortakManager.Mukavemet[3].MaxDeger);
            //var Kaya = Mukavemet.MembershipFunctions.AddRectangle(_ortakManager.Mukavemet[4].Adi, _ortakManager.Mukavemet[4].MinDeger, _ortakManager.Mukavemet[4].MaxDeger);
            //inputs.Add(Mukavemet);

            //var Doygunluk = new LinguisticVariable("Doygunluk");
            //var GazaDoygun = Doygunluk.MembershipFunctions.AddRectangle(_ortakManager.Doygunluk[0].Adi, _ortakManager.Doygunluk[0].MinDeger, _ortakManager.Doygunluk[0].MaxDeger);
            //var Belirsiz = Doygunluk.MembershipFunctions.AddRectangle(_ortakManager.Doygunluk[1].Adi, _ortakManager.Doygunluk[1].MinDeger, _ortakManager.Doygunluk[1].MaxDeger);
            //var SuyaDoygun = Doygunluk.MembershipFunctions.AddRectangle(_ortakManager.Doygunluk[2].Adi, _ortakManager.Doygunluk[2].MinDeger, _ortakManager.Doygunluk[2].MaxDeger);
            //inputs.Add(Doygunluk);
            //#endregion

            //#region Output
            //var Toprak = new LinguisticVariable("Toprak");
            //List<IMembershipFunction> OutputMembershipFunctions = new List<IMembershipFunction>();
            //foreach (var item in ozdirenc)
            //{
            //    OutputMembershipFunctions.Add(Toprak.MembershipFunctions.AddRectangle(item.Adi, item.MinDeger, item.MaxDeger));
            //}
            //#endregion

            //IFuzzyEngine fuzzyEngine = new FuzzyEngineFactory().Default();

            //var rule1 = Rule.If(Ozdirenc.Is("").And(Mukavemet.Is(CokGevsek))).Then(Mukavemet.Is(Orta));

            //foreach (var kuralKumeItem in kuralKume.KuralList)
            //{
            //    foreach (var item in kuralKumeItem.Kurallar)
            //    {
            //        item.
            //    }
            //    var rule = Rule.If(Ozdirenc.Is(InputMembershipFunctions[0]));
            //}



            throw new System.NotImplementedException();
        }

        public double Test(double deger1, double deger2, double deger3)
        {
            
            //#region Inputs
            //var Ozdirenc = new LinguisticVariable("Ozdirenc");
            //var Kil = Ozdirenc.MembershipFunctions.AddRectangle("Kil", 0, 30);
            //var Silt = Ozdirenc.MembershipFunctions.AddRectangle("Silt", 20, 50);
            //var Kum = Ozdirenc.MembershipFunctions.AddRectangle("Kum", 40, 80);
            //var Cakil = Ozdirenc.MembershipFunctions.AddRectangle("Cakil", 70, 100);

            //var Mukavemet = new LinguisticVariable("Mukavemet");
            //var CokGevsek = Mukavemet.MembershipFunctions.AddRectangle("CokGevsek", 0, 200);
            //var Gevsek = Mukavemet.MembershipFunctions.AddRectangle("Gevsek", 200, 300);
            //var Orta = Mukavemet.MembershipFunctions.AddRectangle("Orta", 300, 500);
            //var Siki = Mukavemet.MembershipFunctions.AddRectangle("Siki", 500, 700);
            //var Kaya = Mukavemet.MembershipFunctions.AddRectangle("Kaya", 700, 1000);

            //var Doygunluk = new LinguisticVariable("Doygunluk");
            //var SuyaDoygun = Doygunluk.MembershipFunctions.AddRectangle("SuyaDoygun", 4, 100);
            //var Belirsiz = Doygunluk.MembershipFunctions.AddRectangle("Belirsiz", 2, 4);
            //var GazaDoygun = Doygunluk.MembershipFunctions.AddRectangle("GazaDoygun", 0, 2);
            //#endregion

            //#region Outputs

            //#endregion

            //IFuzzyEngine fuzzyEngine = new FuzzyEngineFactory().Default();

            //var rule1 = Rule.If(Ozdirenc.Is(Kil).And(Mukavemet.Is(CokGevsek))).Then(Mukavemet.Is(Orta));
            //var rule5 = Rule.If(Ozdirenc.Is(Kil).And(Mukavemet.Is(CokGevsek))).Then(Doygunluk.Is(SuyaDoygun));
            //var rule2 = Rule.If(Ozdirenc.Is(Silt).Or(Mukavemet.Is(Gevsek)).Or(Mukavemet.IsNot(Kaya))).Then(Mukavemet.Is(CokGevsek));
            //var rule3 = Rule.If(Ozdirenc.Is(Kum).And(Mukavemet.Is(CokGevsek))).Then(Mukavemet.Is(Orta));
            //var rule4 = Rule.If(Ozdirenc.Is(Cakil).And(Mukavemet.Is(CokGevsek))).Then(Mukavemet.Is(Orta));
            //var rule6 = Rule.If(Ozdirenc.Is(Cakil).And(Ozdirenc.Is(Kum))).Then(Mukavemet.Is(Orta));
            //var rule7 = Rule.If(Ozdirenc.Is(Cakil).And(Doygunluk.Is(SuyaDoygun))).Then(Mukavemet.Is(Gevsek));
            //var rule8 = Rule.If(Ozdirenc.Is(Kil).And(Doygunluk.Is(GazaDoygun))).Then(Mukavemet.Is(Siki));
            //var rule9 = Rule.If(Ozdirenc.Is(Silt).And(Doygunluk.Is(SuyaDoygun))).Then(Mukavemet.Is(Kaya));
            //var rule10 = Rule.If(Ozdirenc.Is(Cakil).And(Doygunluk.Is(GazaDoygun))).Then(Mukavemet.Is(CokGevsek));
            //var rule11 = Rule.If(Ozdirenc.Is(Kum).And(Doygunluk.Is(SuyaDoygun))).Then(Mukavemet.Is(Orta));

            //fuzzyEngine.Rules.Add(rule1, rule2, rule3, rule4, rule5, rule6, rule7, rule8, rule9, rule10, rule11);

            //var result = fuzzyEngine.Defuzzify(new { Ozdirenc = deger1, Mukavemet = deger2, Doygunluk = deger3 });

            return 0;
        }

        private List<DegiskenDTO> GorunenAdDuzenle(List<DegiskenDTO> degisken)
        {
            string TrChar = "ığüşöçĞÜŞİÖÇ";
            string EnChar = "igusocGUSIOC";
            foreach (var item in degisken)
            {
                item.GorunenAdi = item.Adi;
                item.Adi = item.Adi.Replace(" ", "");
                for (int i = 0; i < TrChar.Length; i++)
                {
                    item.Adi.Replace(TrChar[i], EnChar[i]);
                }
            }
            return degisken;
        }

        private string KuralOlustur(FuzzyKuralListDTO kuralList)
        {
            string ruleText = "if ";
            foreach (var item in kuralList.Kurallar)
            {
                if (item.Operator !=null)
                {
                    ruleText = ruleText + ((int)item.Operator == (int)Enums.Operator.And ? " and " : " or ");
                }
                if (item.Ozdirenc != null)
                {
                    ruleText = ruleText + "(Ozdirenc" + (_ortakManager.OperatorList[(int)item.Esitlik - 1].Text) + item.Ozdirenc + ")";
                }
                else if (item.Mukavemet != null)
                {
                    ruleText = ruleText + "(Mukavemet" + (_ortakManager.OperatorList[(int)item.Esitlik - 1].Text) + _ortakManager.MukavemetList[(int)item.Mukavemet - 1].Text + ")";
                }
                else if (item.Doygunluk != null)
                {
                    ruleText = ruleText + "(Mukavemet" + (_ortakManager.OperatorList[(int)item.Esitlik - 1].Text) + _ortakManager.DoygunlukList[(int)item.Doygunluk - 1].Text + ")";
                }
            }
            return ruleText;
        }
    }

    public interface IFuzzyManager : IBaseManager
    {
        double Test(double deger1, double deger2, double deger3);

        void KurallariOlusturFLS(KuralKumeDTO kuralKume);

        void KurallariOlusturFLL(KuralKumeDTO kuralKume);
    }
}
