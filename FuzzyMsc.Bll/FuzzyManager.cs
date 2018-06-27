using FuzzyMsc.Bll.Interface;
using FuzzyMsc.Core.Enums;
using FuzzyMsc.Dto;
using FuzzyMsc.Dto.FuzzyDTOS;
using FuzzyMsc.Entity.Model;
using FuzzyMsc.FuzzyLibrary;
using FuzzyMsc.Pattern.UnitOfWork;
using FuzzyMsc.Service;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace FuzzyMsc.Bll
{
    public class FuzzyManager : IFuzzyManager
    {

        IUnitOfWorkAsync _unitOfWork;
        IOrtakManager _ortakManager;
        IKullaniciService _kullaniciService;
        IKuralService _kuralService;
        IKuralListService _kuralListService;
        IKuralListItemService _kuralListItemService;
        IKuralListTextService _kuralListTextService;
        IDegiskenService _degiskenService;
        IDegiskenItemService _degiskenItemService;
        MamdaniFuzzySystem _fsToprak = null;

        public FuzzyManager(
            IUnitOfWorkAsync unitOfWork,
            IKullaniciService kullaniciService,
            IOrtakManager ortakManager,
            IKuralService kuralService,
            IKuralListService kuralListService,
            IKuralListItemService kuralListItemService,
            IKuralListTextService kuralListTextService,
            IDegiskenService degiskenService,
            IDegiskenItemService degiskenItemService)
        {
            _unitOfWork = unitOfWork;
            _ortakManager = ortakManager;
            _kullaniciService = kullaniciService;
            _kuralService = kuralService;
            _kuralListService = kuralListService;
            _kuralListTextService = kuralListTextService;
            _degiskenService = degiskenService;
            _degiskenItemService = degiskenItemService;
            _kuralListItemService = kuralListItemService;
        }

        public SonucDTO KumeKaydet(KuralKumeDTO kuralKume)
        {
            SonucDTO sonuc = new SonucDTO();
            var ozdirenc = GorunenAdDuzenle(kuralKume.OzdirencList);
            var toprak = GorunenAdDuzenle(kuralKume.ToprakList);

            #region Database Kayit Islemleri

            try
            {
                _unitOfWork.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted);

                #region Kural
                Kural kural = new Kural
                {
                    KuralAdi = kuralKume.KumeAdi,
                    AktifMi = true,
                    EklenmeTarihi = DateTime.Now
                };
                _kuralService.BulkInsert(kural);
                #endregion

                #region KuralListText
                List<KuralListText> kurallar = new List<KuralListText>();
                foreach (var KuralListItem in kuralKume.KuralList)
                {
                    string ruleText = KuralOlustur(KuralListItem);
                    kurallar.Add(new KuralListText { KuralID = kural.KuralID, KuralText = ruleText });
                }
                _kuralListTextService.BulkInsertRange(kurallar);
                #endregion

                #region Input Degisken
                Degisken ozdirencDegisken = new Degisken
                {
                    KuralID = kural.KuralID,
                    DegiskenTipID = (byte)Enums.DegiskenTip.Input,
                    DegiskenAdi = "Özdirenç",
                    DegiskenGorunenAdi = "Ozdirenc"
                };
                _degiskenService.BulkInsert(ozdirencDegisken);
                var ozdirencItem = (from a in ozdirenc
                                    select new DegiskenItem()
                                    {
                                        DegiskenID = ozdirencDegisken.DegiskenID,
                                        DegiskenItemAdi = a.Adi,
                                        DegiskenItemGorunenAdi = a.GorunenAdi,
                                        MinDeger = a.MinDeger,
                                        MaxDeger = a.MaxDeger
                                    });
                _degiskenItemService.BulkInsertRange(ozdirencItem);
                #endregion

                #region Output Degisken
                Degisken toprakDegisken = new Degisken
                {
                    KuralID = kural.KuralID,
                    DegiskenTipID = (byte)Enums.DegiskenTip.Output,
                    DegiskenAdi = "Toprak",
                    DegiskenGorunenAdi = "Toprak"
                };
                _degiskenService.BulkInsert(toprakDegisken);
                var toprakItem = (from a in toprak
                                  select new DegiskenItem()
                                  {
                                      DegiskenID = toprakDegisken.DegiskenID,
                                      DegiskenItemAdi = a.Adi,
                                      DegiskenItemGorunenAdi = a.GorunenAdi,
                                      MinDeger = a.MinDeger,
                                      MaxDeger = a.MaxDeger
                                  });
                _degiskenItemService.BulkInsertRange(toprakItem);
                #endregion



                #region KuralList
                List<KuralListItem> kuralListItem = new List<KuralListItem>();
                for (int i = 0; i < kuralKume.KuralList.Count; i++)
                {
                    var kuralList = (new KuralList { KuralID = kural.KuralID, SiraNo = (byte)(i + 1) });
                    _kuralListService.BulkInsert(kuralList);

                    foreach (var item in kuralKume.KuralList)
                    {
                        var InputDegiskenID = _degiskenItemService.Queryable().FirstOrDefault(d => d.Degisken.DegiskenTipID == (byte)Enums.DegiskenTip.Input && d.DegiskenItemAdi == item.Kural.Ozdirenc).DegiskenItemID;
                        kuralListItem.Add(new KuralListItem { KuralListID = kuralList.KuralListID, DegiskenItemID = InputDegiskenID });

                        var OutputDegiskenID = _degiskenItemService.Queryable().FirstOrDefault(d => d.Degisken.DegiskenTipID == (byte)Enums.DegiskenTip.Output && d.DegiskenItemAdi == item.Kural.Toprak).DegiskenItemID;
                        kuralListItem.Add(new KuralListItem { KuralListID = kuralList.KuralListID, DegiskenItemID = InputDegiskenID });
                    }
                }
                _kuralListItemService.BulkInsertRange(kuralListItem);
                #endregion

                _unitOfWork.Commit();
                sonuc.Sonuc = true;
                sonuc.Mesaj = "Kural Kümesi Başarı İle Kaydedildi.";
                sonuc.Nesne = null;
                return sonuc;
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                sonuc.Sonuc = false;
                sonuc.Mesaj = "Kural Kümesi Kaydedilirken Hata Oluştu. Hata Açıklaması: " + ex.Message;
                sonuc.Nesne = null;
                return sonuc;
            }

            #endregion
        }

        public string FuzzyKuralOlusturVeSonucGetirFLL(KuralGetirDTO kurallar, double inputValue)
        {
            //var ozdirenc = GorunenAdDuzenle(kuralKume.OzdirencList);
            //var toprak = GorunenAdDuzenle(kuralKume.ToprakList);
            FuzzySystemResultDTO system = new FuzzySystemResultDTO();
            system = SistemOlustur(kurallar, inputValue);
            _fsToprak = system.System;
            inputValue = system.InputValue;


            FuzzyVariable fvInput = _fsToprak.InputByName(kurallar.DegiskenList.FirstOrDefault(d => d.DegiskenTipID == (byte)Enums.DegiskenTip.Input).DegiskenGorunenAdi);
            FuzzyVariable fvOutput = _fsToprak.OutputByName(kurallar.DegiskenList.FirstOrDefault(d => d.DegiskenTipID == (byte)Enums.DegiskenTip.Output).DegiskenGorunenAdi);

            Dictionary<FuzzyVariable, double> inputValues = new Dictionary<FuzzyVariable, double>();
            inputValues.Add(fvInput, inputValue);

            Dictionary<FuzzyVariable, double> result = _fsToprak.Calculate(inputValues);
            _fsToprak.DefuzzificationMethod = DefuzzificationMethod.Centroid;

            double outputValue = result[fvOutput];
            string outputType = SonucGetirYakinSiniraGore(kurallar, outputValue);

            return outputType;

            #region FuzzyIslemleri
            //#region Inputs
            //FuzzyVariable fvOzdirenc = new FuzzyVariable("Ozdirenc", 0.0, 1000.0);
            //foreach (var item in ozdirenc)
            //{
            //    fvOzdirenc.Terms.Add(new FuzzyTerm(item.Adi, new TriangularMembershipFunction(item.MinDeger, (item.MinDeger + item.MaxDeger) / 2, item.MaxDeger)));
            //}
            //fsToprak.Input.Add(fvOzdirenc);

            //FuzzyVariable fvMukavemet = new FuzzyVariable("Mukavemet", 0.0, 1000.0);
            //foreach (var item in _ortakManager.Mukavemet)
            //{
            //    fvMukavemet.Terms.Add(new FuzzyTerm(item.Adi, new TriangularMembershipFunction(item.MinDeger, (item.MinDeger + item.MaxDeger) / 2, item.MaxDeger)));
            //}
            //fsToprak.Input.Add(fvMukavemet);

            //FuzzyVariable fvDoygunluk = new FuzzyVariable("Doygunluk", 0.0, 10.0);
            //foreach (var item in _ortakManager.Doygunluk)
            //{
            //    fvDoygunluk.Terms.Add(new FuzzyTerm(item.Adi, new TriangularMembershipFunction(item.MinDeger, (item.MinDeger + item.MaxDeger) / 2, item.MaxDeger)));
            //}
            //fsToprak.Input.Add(fvDoygunluk);
            //#endregion

            //#region Output
            //FuzzyVariable fvToprak = new FuzzyVariable("Toprak", 0.0, 1000.0);
            //foreach (var item in toprak)
            //{
            //    fvToprak.Terms.Add(new FuzzyTerm(item.Adi, new TriangularMembershipFunction(item.MinDeger, (item.MinDeger + item.MaxDeger) / 2, item.MaxDeger)));
            //}
            //fsToprak.Output.Add(fvToprak);
            //#endregion
            //List<string> kurallar = new List<string>();

            //foreach (var KuralListItem in kuralKume.KuralList)
            //{
            //    string ruleText = KuralOlustur(KuralListItem) + " then (Toprak is " + KuralListItem.Sonuc + ")";
            //    kurallar.Add(ruleText);
            //}
            #endregion            
        }

        public bool FuzzyKuralOlusturVeSonucGetirFLLKarsilastirma(KuralGetirDTO kurallar, double inputValue1, double inputValue2, int oran)
        {
            FuzzySystemResultDTO system = new FuzzySystemResultDTO();
            system = SistemOlustur(kurallar, inputValue1);
            _fsToprak = system.System;
            inputValue1 = system.InputValue;

            FuzzyVariable fvInput1 = _fsToprak.InputByName(kurallar.DegiskenList.FirstOrDefault(d => d.DegiskenTipID == (byte)Enums.DegiskenTip.Input).DegiskenGorunenAdi);
            FuzzyVariable fvOutput1 = _fsToprak.OutputByName(kurallar.DegiskenList.FirstOrDefault(d => d.DegiskenTipID == (byte)Enums.DegiskenTip.Output).DegiskenGorunenAdi);

            Dictionary<FuzzyVariable, double> inputValues1 = new Dictionary<FuzzyVariable, double>();
            inputValues1.Add(fvInput1, inputValue1);

            Dictionary<FuzzyVariable, double> result1 = _fsToprak.Calculate(inputValues1);
            _fsToprak.DefuzzificationMethod = DefuzzificationMethod.Bisector;

            double outputValue1 = result1[fvOutput1];

            _fsToprak = null;
            system = SistemOlustur(kurallar, inputValue2);
            _fsToprak = system.System;
            inputValue2 = system.InputValue;

            FuzzyVariable fvInput2 = _fsToprak.InputByName(kurallar.DegiskenList.FirstOrDefault(d => d.DegiskenTipID == (byte)Enums.DegiskenTip.Input).DegiskenGorunenAdi);
            FuzzyVariable fvOutput2 = _fsToprak.OutputByName(kurallar.DegiskenList.FirstOrDefault(d => d.DegiskenTipID == (byte)Enums.DegiskenTip.Output).DegiskenGorunenAdi);

            Dictionary<FuzzyVariable, double> inputValues2 = new Dictionary<FuzzyVariable, double>();
            inputValues2.Add(fvInput2, inputValue2);

            Dictionary<FuzzyVariable, double> result2 = _fsToprak.Calculate(inputValues2);
            _fsToprak.DefuzzificationMethod = DefuzzificationMethod.Centroid;

            double outputValue2 = result2[fvOutput2];

            var result = SonucGetirYakinligaGore(outputValue1, outputValue2, oran);

            return result;
        }

        private FuzzySystemResultDTO SistemOlustur(KuralGetirDTO kurallar, double inputValue)
        {
            FuzzySystemResultDTO result = new FuzzySystemResultDTO();
            MamdaniFuzzySystem fsToprak = new MamdaniFuzzySystem();

            foreach (var degisken in kurallar.DegiskenList)
            {
                if (degisken.DegiskenTipID == (byte)Enums.DegiskenTip.Input)
                {
                    
                    FuzzyVariable fvInput = new FuzzyVariable(degisken.DegiskenGorunenAdi, 0.0, 1000.0);
                    var degiskenItemler = kurallar.DegiskenItemList.Where(k => k.DegiskenID == degisken.DegiskenID).ToList();
                    for (int i = 0; i < degiskenItemler.Count; i++)
                    {
                        if (inputValue == degiskenItemler[i].MinDeger)
                        {
                            inputValue++;
                        }
                        double maxValue;
                        if (i != degiskenItemler.Count - 1)
                        {
                            if (degiskenItemler[i].MaxDeger == degiskenItemler[i + 1].MinDeger)
                                maxValue = degiskenItemler[i].MaxDeger - 1;
                            else
                                maxValue = degiskenItemler[i].MaxDeger;
                        }
                        else
                            maxValue = degiskenItemler[i].MaxDeger;

                        fvInput.Terms.Add(new FuzzyTerm(degiskenItemler[i].DegiskenItemGorunenAdi, new TriangularMembershipFunction(degiskenItemler[i].MinDeger, (degiskenItemler[i].MinDeger + degiskenItemler[i].MaxDeger) / 2, maxValue)));
                    }
                    fsToprak.Input.Add(fvInput);
                }
                else
                {
                    FuzzyVariable fvOutput = new FuzzyVariable(degisken.DegiskenGorunenAdi, 0.0, 1000.0);
                    var degiskenItemler = kurallar.DegiskenItemList.Where(k => k.DegiskenID == degisken.DegiskenID).ToList();
                    for (int i = 0; i < degiskenItemler.Count; i++)
                    {
                        double maxValue;
                        if (i != degiskenItemler.Count - 1)
                        {
                            if (degiskenItemler[i].MaxDeger == degiskenItemler[i + 1].MinDeger)
                                maxValue = degiskenItemler[i].MaxDeger - 1;
                            else
                                maxValue = degiskenItemler[i].MaxDeger;
                        }
                        else
                            maxValue = degiskenItemler[i].MaxDeger;

                        fvOutput.Terms.Add(new FuzzyTerm(degiskenItemler[i].DegiskenItemGorunenAdi, new TriangularMembershipFunction(degiskenItemler[i].MinDeger, (degiskenItemler[i].MinDeger + degiskenItemler[i].MaxDeger) / 2, maxValue)));
                    }
                    fsToprak.Output.Add(fvOutput);
                }
            }

            foreach (var kuralText in kurallar.KuralListText)
            {
                MamdaniFuzzyRule rule = fsToprak.ParseRule(kuralText.KuralText);
                fsToprak.Rules.Add(rule);
            }

            result.System = fsToprak;
            result.InputValue = inputValue;
            return result;
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
                item.GorunenAdi = item.Adi.Replace(" ", "");
                for (int i = 0; i < TrChar.Length; i++)
                {
                    item.GorunenAdi = item.GorunenAdi.Replace(TrChar[i], EnChar[i]);
                }
                //        var unaccentedText = String.Join("", item.Adi.Normalize(NormalizationForm.FormD)
                //.Where(c => char.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark));
            }
            return degisken;
        }

        private string KuralOlustur(KuralListDTO kuralList)
        {
            var ozdirenc = TurkceKarakter(kuralList.Kural.Ozdirenc);
            var toprak = TurkceKarakter(kuralList.Kural.Toprak);

            return "if (Ozdirenc is " + ozdirenc + ") then (Toprak is " + toprak + ")";

        }

        private string TurkceKarakter(string text)
        {
            string TrChar = "ığüşöçĞÜŞİÖÇ";
            string EnChar = "igusocGUSIOC";
            for (int i = 0; i < TrChar.Length; i++)
            {
                text = text.Replace(TrChar[i], EnChar[i]);
            }

            return text.Replace(" ", "");
        }

        public KuralGetirDTO KuralGetir(long kuralID)
        {
            KuralGetirDTO kuralGetir = new KuralGetirDTO();
            List<DegiskenItem> degiskenItemList = new List<DegiskenItem>();
            var kural = _kuralService.Queryable().FirstOrDefault(k => k.KuralID == kuralID && k.AktifMi == true);
            var kuralListText = kural.KuralListTexts.ToList();
            var degiskenler = kural.Degiskens.ToList();
            foreach (var item in degiskenler)
            {
                var degiskenItems = _degiskenItemService.Queryable().Where(d => d.DegiskenID == item.DegiskenID).ToList();
                degiskenItemList.AddRange(degiskenItems);
            }

            kuralGetir.Kural = kural;
            kuralGetir.KuralListText = kuralListText;
            kuralGetir.DegiskenList = degiskenler;
            kuralGetir.DegiskenItemList = degiskenItemList;

            return kuralGetir;
        }

        private string SonucGetirYakinSiniraGore(KuralGetirDTO kurallar, double outputValue)
        {
            string sonuc = "";

            var degiskenID = kurallar.DegiskenList.FirstOrDefault(dl => dl.DegiskenTipID == (byte)Enums.DegiskenTip.Output).DegiskenID;
            var OutputList = _degiskenItemService.Queryable().Where(d => d.DegiskenID == degiskenID).ToList();

            for (int i = 0; i < OutputList.Count; i++)
            {
                if (i == OutputList.Count - 1)
                {
                    //if (outputValue >= OutputList[i].MinDeger && outputValue <= OutputList[i].MaxDeger)
                    //{
                    sonuc = OutputList[i].DegiskenItemAdi;
                    break;
                    //}                    
                }
                else
                {
                    if (OutputList[i].MaxDeger > OutputList[i + 1].MinDeger) //Bir sonraki tanım aralığı ile kesişimi var demektir
                    {
                        if (outputValue <= OutputList[i].MaxDeger && outputValue >= OutputList[i + 1].MinDeger)
                        {
                            sonuc = Math.Abs(outputValue - OutputList[i].MaxDeger) > Math.Abs(outputValue - OutputList[i + 1].MinDeger) ? OutputList[i].DegiskenItemAdi : OutputList[i + 1].DegiskenItemAdi;
                            break;
                        }
                    }
                    else
                    {
                        if (outputValue >= OutputList[i].MinDeger && outputValue <= OutputList[i].MaxDeger)
                        {
                            sonuc = OutputList[i].DegiskenItemAdi;
                            break;
                        }
                    }
                }
            }


            return sonuc;
        }

        private bool SonucGetirYakinligaGore(double outputValue1, double outputValue2, int oran)
        {
            if (outputValue1 > outputValue2)
            {
                if (outputValue1 * oran / 100 > outputValue2)
                {
                    return false;
                }
            }
            else if (outputValue2 > outputValue1)
            {
                if (outputValue2 * oran / 100 > outputValue1)
                {
                    return false;
                }
            }
            else
            {
                return true;
            }

            return true;
        }
    }

    public interface IFuzzyManager : IBaseManager
    {
        double Test(double deger1, double deger2, double deger3);

        void KurallariOlusturFLS(KuralKumeDTO kuralKume);

        string FuzzyKuralOlusturVeSonucGetirFLL(KuralGetirDTO kurallar, double inputValue);

        bool FuzzyKuralOlusturVeSonucGetirFLLKarsilastirma(KuralGetirDTO kurallar, double inputValue1, double inputValue2, int oran);

        SonucDTO KumeKaydet(KuralKumeDTO kuralKume);

        KuralGetirDTO KuralGetir(long kuralID);
    }
}
