using FuzzyMsc.Bll.Interface;
using FuzzyMsc.Core.Enums;
using FuzzyMsc.Dto;
using FuzzyMsc.Dto.GraphDTOS;
using FuzzyMsc.Dto.FuzzyDTOS;
using FuzzyMsc.Dto.HighchartsDTOS;
using FuzzyMsc.Pattern.UnitOfWork;
using FuzzyMsc.Service;
using Microsoft.Office.Interop.Excel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace FuzzyMsc.Bll
{
    public class GraphManager : IGraphManager
    {
        IUnitOfWorkAsync _unitOfWork;
        ICommonManager _commonManager;
        IUserService _userService;
        IRuleService _ruleService;
        IRuleListService _ruleListService;
        IRuleListItemService _ruleListItemService;
        IRuleListTextService _ruleListTextService;
        IVariableService _variableService;
        IVariableItemService _variableItemService;
        IFuzzyManager _fuzzyManager;

        private List<List<ResistivityDTO>> rezGenelList;
        private List<List<SeismicDTO>> sisGenelList;
        private List<List<DrillDTO>> sonGenelList;
        private GraphCountDTO cizimCount = new GraphCountDTO();
        private List<GraphDetailedDTO> cizimDetailedList = new List<GraphDetailedDTO>();
        private List<SeriesDTO> datasetList = new List<SeriesDTO>();
        private int id;
        Microsoft.Office.Interop.Excel.Application xl;
        Microsoft.Office.Interop.Excel.Workbook xlWorkbook;

        [DllImport("user32.dll")]
        static extern int GetWindowThreadProcessId(int hWnd, out int lpdwProcessId);

        public GraphManager(
            IUnitOfWorkAsync unitOfWork,
            IUserService userService,
            ICommonManager commonManager,
            IRuleService ruleService,
            IRuleListService ruleListService,
            IRuleListItemService ruleListItemService,
            IRuleListTextService ruleListTextService,
            IVariableService variableService,
            IVariableItemService variableItemService,
            IFuzzyManager fuzzyManager)
        {
            _unitOfWork = unitOfWork;
            _commonManager = commonManager;
            _userService = userService;
            _ruleService = ruleService;
            _ruleListService = ruleListService;
            _ruleListTextService = ruleListTextService;
            _variableService = variableService;
            _variableItemService = variableItemService;
            _ruleListItemService = ruleListItemService;
            _fuzzyManager = fuzzyManager;
        }

        public ResultDTO ExcelKontrolEt(ExcelModelDTO excel, string path)
        {
            ResultDTO sonuc = new ResultDTO();
            try
            {
                sonuc.Result = true;
                File.WriteAllBytes(path, Convert.FromBase64String(excel.data));
            }
            catch (Exception ex)
            {
                sonuc.Result = false;
            }
            return sonuc;


        }
        public ResultDTO GraphOlustur(GraphDTO graph, string path)
        {
            try
            {
                ResultDTO sonuc = new ResultDTO();
                File.WriteAllBytes(path, Convert.FromBase64String(graph.excel.data));
                xl = new Microsoft.Office.Interop.Excel.Application();
                xlWorkbook = xl.Workbooks.Open(path);
                GetWindowThreadProcessId(xl.Hwnd, out id);
                HighchartsDTO highcharts = new HighchartsDTO();

                #region Rezistivite
                RezistiviteOlustur(highcharts, xlWorkbook);
                #endregion

                #region Sismik
                SismikOlustur(highcharts, xlWorkbook);
                #endregion

                #region Sondaj
                SondajOlustur(highcharts, xlWorkbook);
                #endregion

                //highcharts.series.AddRange(GraphDataOlustur(sisGenelList));
                //highcharts.series.AddRange(GraphDataOlustur(sonGenelList));
                //highcharts.series.AddRange(GraphDataOlustur(rezGenelList));
                SectionDTO kesitDTO = new SectionDTO { ResistivityGeneralList = rezGenelList, SeismicGeneralList = sisGenelList, DrillGeneralList = sonGenelList };
                highcharts.series.AddRange(GraphDataOlustur(graph.ruleId, kesitDTO, graph.parameters));

                bool fayVarMi = highcharts.series.Any(s => s.name == "Fay");

                if (fayVarMi)
                    highcharts.series.AddRange(GraphDataOlustur(graph.ruleId, kesitDTO, graph.parameters));

                highcharts.series = highcharts.series.Distinct().ToList();

                double minX = MinHesapla(highcharts);
                highcharts.xAxis = new AxisDTO { min = 0, minTickInterval = (int)graph.parameters.ScaleX, offset = 20, title = new AxisTitleDTO { text = "Genişlik" }, labels = new AxisLabelsDTO { format = "{value} m" } };
                highcharts.yAxis = new AxisDTO { min = (int)minX - 5, minTickInterval = (int)graph.parameters.ScaleY, offset = 20, title = new AxisTitleDTO { text = "Yükseklik" }, labels = new AxisLabelsDTO { format = "{value} m" } };

                highcharts.parameters = graph.parameters;
                //highcharts.sayilar = cizimCount;
                //highcharts.sayilar.basariOrani = BasariHesapla(cizimCount, graph.sayilar);
                highcharts.cizimBilgileri = cizimDetailedList;

                sonuc.Object = highcharts;
                sonuc.Result = true;
                return sonuc;
            }
            finally
            {
                xlWorkbook.Close();
                xl.Quit();
                Process process = Process.GetProcessById(id);
                process.Kill();

            }
        }

        private double BasariHesapla(GraphCountDTO cizimCount, GraphCountDTO varsayilanCount)
        {
            double oran = 100.0;

            int normalFarki = Math.Abs(cizimCount.Normal - varsayilanCount.Normal);
            int kapatmaFarki = Math.Abs(cizimCount.Closure - varsayilanCount.Closure);
            int fayFarki = Math.Abs(cizimCount.Fault - varsayilanCount.Fault);

            for (int i = 0; i < fayFarki; i++)
            {
                oran = oran - (oran * 15 / 100);
            }
            for (int i = 0; i < kapatmaFarki; i++)
            {
                oran = oran - (oran * 7.5 / 100);
            }
            for (int i = 0; i < normalFarki; i++)
            {
                oran = oran - (oran * 1 / 100);
            }

            oran = oran - (oran * 1 / 100); //%1 oranında çizimlerden kaynaklı sapma

            oran = oran - (oran * (normalFarki + kapatmaFarki + fayFarki) / 100); //Şekil sayılarının farkına göre genel sapma

            return oran;
        }

        private double MinHesapla(HighchartsDTO highcharts)
        {
            double min = Double.MaxValue;
            //foreach (var item in highcharts.series)
            //{
            //    foreach (var dataItem in item.data)
            //    {
            //        if (dataItem != null)
            //        {
            //            if (dataItem.Count > 0)
            //            {
            //                double a = dataItem[1];
            //                if (a < min)
            //                    min = a;
            //            }
            //        }
            //    }

            //}
            foreach (var item in highcharts.annotations)
            {
                var minItem = (double)item.labels.Min(m => m.point.y);
                if (minItem != 0 && minItem < min)
                    min = minItem;
            }
            return min;
        }

        private void RezistiviteOlustur(HighchartsDTO highcharts, Workbook xlWorkbook)
        {
            rezGenelList = new List<List<ResistivityDTO>>();
            List<ResistivityDTO> rezList = new List<ResistivityDTO>();
            ResistivityDTO rezItem = new ResistivityDTO();
            Microsoft.Office.Interop.Excel._Worksheet xlWorksheetRezistivite = (Microsoft.Office.Interop.Excel._Worksheet)xlWorkbook.Sheets[1];
            Microsoft.Office.Interop.Excel.Range xlRange = xlWorksheetRezistivite.UsedRange;
            #region Tablo Satir ve Sutun Genislikleri
            int rowCount = xlRange.Rows.Count;
            int colCount = xlRange.Columns.Count;
            for (int i = 1; i <= rowCount; i++)
            {
                if (string.IsNullOrEmpty((xlWorksheetRezistivite.Cells[i + 1, 1]).Value))
                {
                    rowCount = i;
                    break;
                }
            }
            #endregion

            #region Derinlik Eşitleme

            List<List<ExcelDTO>> rezExcel = new List<List<ExcelDTO>>();
            List<ExcelDTO> rezExcelItem;
            #region Tüm Veriler Aktarılıyor
            for (int i = 2; i < rowCount + 1; i++)
            {
                rezExcelItem = new List<ExcelDTO>();
                for (int j = 1; j < colCount + 1; j++)
                {
                    ExcelDTO Instance;
                    if ((xlWorksheetRezistivite.Cells[i, j]).Value == null) //Boş olan hücrelerde hata verdiği için kontrol yapılıyor
                    {
                        Instance = new ExcelDTO { TypeID = (byte)Enums.ExcelDataType.Real, Value = "" };
                        rezExcelItem.Add(Instance);
                    }
                    else
                    {
                        var value = (string)(xlWorksheetRezistivite.Cells[i, j]).Value.ToString();
                        Instance = new ExcelDTO { TypeID = (byte)Enums.ExcelDataType.Real, Value = value };
                        rezExcelItem.Add(Instance);
                    }

                }
                rezExcel.Add(rezExcelItem);
            }
            #endregion

            #region Kaydırmalar Yapılarak Yapay Veriler Ekleniyor
            foreach (var item in rezExcel)
            {
                if (item[item.Count - 1].Value == "" && item[item.Count - 2].Value == "") //Son İki Hücre Boş İse Kaydırma Yapılacak
                {
                    for (int i = 0; i < item.Count; i++)
                    {
                        if (item[i].Value == "" && item[i + 1].Value == "")//İlk Boşluk Olan İkili Hücre bulunuyor
                        {
                            item[i - 2].JSONData = JsonConvert.SerializeObject(item[i - 2]);
                            item[i - 1].JSONData = JsonConvert.SerializeObject(item[i - 1]);

                            List<ExcelDTO> finalItem = new List<ExcelDTO>();//Sona atılacak değerler 
                            finalItem.Add(JsonConvert.DeserializeObject<ExcelDTO>(item[i - 2].JSONData));
                            finalItem.Add(JsonConvert.DeserializeObject<ExcelDTO>(item[i - 1].JSONData));

                            for (int j = i; j < item.Count; j = j + 2)//İçi dolu olan son hücreden itibaren sona kadar kaydırma yapılıyor
                            {
                                item[j - 2].JSONData = JsonConvert.SerializeObject(item[i - 4]);
                                item[j - 1].JSONData = JsonConvert.SerializeObject(item[i - 3]);
                                item[j - 2] = JsonConvert.DeserializeObject<ExcelDTO>(item[j - 2].JSONData);
                                item[j - 1] = JsonConvert.DeserializeObject<ExcelDTO>(item[j - 1].JSONData);
                                item[j - 4].TypeID = (byte)Enums.ExcelDataType.Artificial;
                                item[j - 3].TypeID = (byte)Enums.ExcelDataType.Artificial;

                                if (j == item.Count - 2)
                                {
                                    item[j].JSONData = JsonConvert.SerializeObject(finalItem[0]); //Son değerlerin kaydırılması
                                    item[j + 1].JSONData = JsonConvert.SerializeObject(finalItem[1]);
                                    item[j] = JsonConvert.DeserializeObject<ExcelDTO>(item[j].JSONData);
                                    item[j + 1] = JsonConvert.DeserializeObject<ExcelDTO>(item[j + 1].JSONData);
                                    item[j - 2].TypeID = (byte)Enums.ExcelDataType.Real;//Sondan önceki değerlerin gerçek hale getirilmesi
                                    item[j - 1].TypeID = (byte)Enums.ExcelDataType.Real;
                                    continue;
                                }
                            }
                            break;
                        }
                    }
                }
            }
            #endregion

            #endregion

            #region Derinlik Eşitlemesiz Kodlar
            //for (int i = 1; i < rowCount; i++)
            //{
            //    rezItem = new RezistiviteDTO();
            //    rezItem.ID = i;
            //    rezItem.Adi = (string)(xlWorksheetRezistivite.Cells[i + 1, 1]).Value.ToString();
            //    rezItem.X = (double)(xlWorksheetRezistivite.Cells[i + 1, 2]).Value;
            //    rezItem.K = (double)(xlWorksheetRezistivite.Cells[i + 1, 4]).Value;
            //    rezList.Add(rezItem);
            //}
            //rezGenelList.Add(rezList);


            //for (int j = 5; j <= colCount; j = j + 2)
            //{
            //    rezList = new List<RezistiviteDTO>();
            //    for (int i = 1; i <= rowCount; i++)
            //    {
            //        if ((xlWorksheetRezistivite.Cells[i + 1, j]).Value == null && (xlWorksheetRezistivite.Cells[i + 1, j + 1]).Value == null) //Exceldeki İki Hücre değeri de boşsa (hem koordinat hem de özdirenç)
            //        {
            //            continue;
            //        }
            //        if ((xlWorksheetRezistivite.Cells[i + 1, j + 1]).Value == null)
            //        {
            //            continue;
            //        }
            //        if ((xlWorksheetRezistivite.Cells[i + 1, j]).Value == null && (xlWorksheetRezistivite.Cells[i + 1, j + 1]).Value != null)//
            //        {
            //            rezItem = new RezistiviteDTO();                        
            //            rezItem.ID = i;
            //            rezItem.Adi = (string)(xlWorksheetRezistivite.Cells[i + 1, 1]).Value.ToString() + count.ToString();
            //            rezItem.X = (double)(xlWorksheetRezistivite.Cells[i + 1, 2]).Value;
            //            rezItem.K = ((double)(xlWorksheetRezistivite.Cells[i + 1, 4]).Value - (double)(xlWorksheetRezistivite.Cells[i + 1, j - 2]).Value) * 0.99;
            //            rezItem.R = (xlWorksheetRezistivite.Cells[i + 1, j + 1]).Value == null ? 0 : (double)(xlWorksheetRezistivite.Cells[i + 1, j + 1]).Value;
            //            rezList.Add(rezItem);
            //            count++;
            //            continue;
            //        }
            //        rezItem = new RezistiviteDTO();
            //        rezItem.ID = i;
            //        rezItem.Adi = (string)(xlWorksheetRezistivite.Cells[i + 1, 1]).Value.ToString() + count.ToString();
            //        rezItem.X = (double)(xlWorksheetRezistivite.Cells[i + 1, 2]).Value;
            //        rezItem.K = (xlWorksheetRezistivite.Cells[i + 1, j]).Value == null ? 0 : (double)(xlWorksheetRezistivite.Cells[i + 1, 4]).Value - (double)(xlWorksheetRezistivite.Cells[i + 1, j]).Value;
            //        rezItem.R = (xlWorksheetRezistivite.Cells[i + 1, j + 1]).Value == null ? 0 : (double)(xlWorksheetRezistivite.Cells[i + 1, j + 1]).Value;
            //        rezList.Add(rezItem);
            //        count++;
            //    }
            //    rezGenelList.Add(rezList);
            //    count = 1;
            //} 
            #endregion

            for (int i = 0; i < rowCount - 1; i++)
            {
                rezItem = new ResistivityDTO();
                rezItem.ID = i + 1;
                rezItem.Name = rezExcel[i][0].Value.ToString();
                rezItem.X = Convert.ToDouble(rezExcel[i][1].Value);
                rezItem.K = Convert.ToDouble(rezExcel[i][3].Value);
                rezItem.TypeID = rezExcel[i][0].TypeID;
                rezList.Add(rezItem);
            }
            rezGenelList.Add(rezList);

            int count = 0;
            for (int j = 4; j < colCount; j = j + 2)
            {
                count++;
                rezList = new List<ResistivityDTO>();
                for (int i = 0; i < rowCount - 1; i++)
                {
                    var rezExcelInstance = rezExcel[i];

                    if (rezExcelInstance[j].Value == "" && rezExcelInstance[j + 1].Value == "") //Exceldeki İki Hücre değeri de boşsa (hem koordinat hem de özdirenç)
                    {
                        continue;
                    }
                    if (rezExcelInstance[j + 1].Value == "")
                    {
                        continue;
                    }
                    if (rezExcelInstance[j].Value == "" && rezExcelInstance[j + 1].Value != "")//Sadece Derinlik Değeri Boşsa
                    {
                        rezItem = new ResistivityDTO();
                        rezItem.ID = i + 1;
                        rezItem.Name = rezExcelInstance[0].Value.ToString() + count.ToString();
                        rezItem.X = Convert.ToDouble(rezExcelInstance[1].Value);
                        var value = "";
                        for (int k = 0; k < rezExcelInstance.Count; k = k + 2)
                        {
                            if (rezExcelInstance[j - (2 + k)].TypeID == (byte)Enums.ExcelDataType.Real)
                            {
                                value = rezExcelInstance[j - (2 + k)].Value;
                                break;
                            }
                        }
                        rezItem.K = (Convert.ToDouble(rezExcelInstance[3].Value) - Convert.ToDouble(value)) * 0.99;
                        rezItem.R = rezExcelInstance[j + 1].Value == "" ? Convert.ToDouble("") : Convert.ToDouble(rezExcelInstance[j + 1].Value);
                        rezItem.TypeID = rezExcelInstance[j].TypeID;
                        rezList.Add(rezItem);
                        continue;
                    }
                    rezItem = new ResistivityDTO();
                    rezItem.ID = i + 1;
                    rezItem.Name = rezExcelInstance[0].Value.ToString() + count.ToString();
                    rezItem.X = Convert.ToDouble(rezExcelInstance[1].Value);
                    rezItem.K = rezExcelInstance[j].Value == "" ? 0 : Convert.ToDouble(rezExcelInstance[3].Value) - Convert.ToDouble(rezExcelInstance[j].Value);
                    rezItem.R = rezExcelInstance[j + 1].Value == "" ? Convert.ToDouble("") : Convert.ToDouble(rezExcelInstance[j + 1].Value);
                    rezItem.TypeID = rezExcelInstance[j].TypeID;
                    rezList.Add(rezItem);

                }
                rezGenelList.Add(rezList);
            }

            highcharts = ChartOlustur(highcharts, rezGenelList);
        }
        private void SismikOlustur(HighchartsDTO highcharts, Workbook xlWorkbook)
        {
            sisGenelList = new List<List<SeismicDTO>>();
            List<SeismicDTO> sisList = new List<SeismicDTO>();
            SeismicDTO sisItem = new SeismicDTO();
            Microsoft.Office.Interop.Excel._Worksheet xlWorksheetSismik = (Microsoft.Office.Interop.Excel._Worksheet)xlWorkbook.Sheets[2];
            Microsoft.Office.Interop.Excel.Range xlRange = xlWorksheetSismik.UsedRange;
            #region Tablo Satir ve Sutun Genislikleri
            int rowCount = xlRange.Rows.Count;
            int colCount = xlRange.Columns.Count;
            for (int i = 1; i <= rowCount; i++)
            {
                if (string.IsNullOrEmpty((xlWorksheetSismik.Cells[i + 1, 1]).Value))
                {
                    rowCount = i;
                    break;
                }
            }
            #endregion

            #region Derinlik Eşitleme

            List<List<ExcelDTO>> sisExcel = new List<List<ExcelDTO>>();
            List<ExcelDTO> sisExcelItem;
            #region Tüm Veriler Aktarılıyor
            for (int i = 2; i < rowCount + 1; i++)
            {
                sisExcelItem = new List<ExcelDTO>();
                for (int j = 1; j < colCount + 1; j++)
                {
                    ExcelDTO Instance;
                    if ((xlWorksheetSismik.Cells[i, j]).Value == null) //Boş olan hücrelerde hata verdiği için kontrol yapılıyor
                    {
                        Instance = new ExcelDTO { TypeID = (byte)Enums.ExcelDataType.Real, Value = "" };
                        sisExcelItem.Add(Instance);
                    }
                    else
                    {
                        var value = (string)(xlWorksheetSismik.Cells[i, j]).Value.ToString();
                        Instance = new ExcelDTO { TypeID = (byte)Enums.ExcelDataType.Real, Value = value };
                        sisExcelItem.Add(Instance);
                    }

                }
                sisExcel.Add(sisExcelItem);
            }
            #endregion

            #region Kaydırmalar Yapılarak Yapay Veriler Ekleniyor
            foreach (var item in sisExcel)
            {
                if (item[item.Count - 1].Value == "" && item[item.Count - 2].Value == "" && item[item.Count - 3].Value == "") //Son İki Hücre Boş İse Kaydırma Yapılacak
                {
                    for (int i = 0; i < item.Count; i++)
                    {
                        if (item[i].Value == "" && item[i + 1].Value == "" && item[i + 2].Value == "")//İlk Boşluk Olan İkili Hücre bulunuyor
                        {
                            item[i - 3].JSONData = JsonConvert.SerializeObject(item[i - 3]);
                            item[i - 2].JSONData = JsonConvert.SerializeObject(item[i - 2]);
                            item[i - 1].JSONData = JsonConvert.SerializeObject(item[i - 1]);

                            List<ExcelDTO> finalItem = new List<ExcelDTO>();//Sona atılacak değerler 
                            finalItem.Add(JsonConvert.DeserializeObject<ExcelDTO>(item[i - 3].JSONData));
                            finalItem.Add(JsonConvert.DeserializeObject<ExcelDTO>(item[i - 2].JSONData));
                            finalItem.Add(JsonConvert.DeserializeObject<ExcelDTO>(item[i - 1].JSONData));

                            for (int j = i; j < item.Count; j = j + 3)//İçi dolu olan son hücreden itibaren sona kadar kaydırma yapılıyor
                            {
                                item[j - 3].JSONData = JsonConvert.SerializeObject(item[i - 6]);
                                item[j - 2].JSONData = JsonConvert.SerializeObject(item[i - 5]);
                                item[j - 1].JSONData = JsonConvert.SerializeObject(item[i - 4]);
                                item[j - 3] = JsonConvert.DeserializeObject<ExcelDTO>(item[j - 3].JSONData);
                                item[j - 2] = JsonConvert.DeserializeObject<ExcelDTO>(item[j - 2].JSONData);
                                item[j - 1] = JsonConvert.DeserializeObject<ExcelDTO>(item[j - 1].JSONData);
                                item[j - 6].TypeID = (byte)Enums.ExcelDataType.Artificial;
                                item[j - 5].TypeID = (byte)Enums.ExcelDataType.Artificial;
                                item[j - 4].TypeID = (byte)Enums.ExcelDataType.Artificial;

                                if (j == item.Count - 3)
                                {
                                    item[j].JSONData = JsonConvert.SerializeObject(finalItem[0]);//Son değerlerin kaydırılması
                                    item[j + 1].JSONData = JsonConvert.SerializeObject(finalItem[1]);
                                    item[j + 2].JSONData = JsonConvert.SerializeObject(finalItem[2]);
                                    item[j] = JsonConvert.DeserializeObject<ExcelDTO>(item[j].JSONData);
                                    item[j + 1] = JsonConvert.DeserializeObject<ExcelDTO>(item[j + 1].JSONData);
                                    item[j + 2] = JsonConvert.DeserializeObject<ExcelDTO>(item[j + 2].JSONData);
                                    item[j - 3].TypeID = (byte)Enums.ExcelDataType.Real;//Sondan önceki değerlerin gerçek hale getirilmesi
                                    item[j - 2].TypeID = (byte)Enums.ExcelDataType.Real;
                                    item[j - 1].TypeID = (byte)Enums.ExcelDataType.Real;
                                    continue;
                                }
                            }
                            break;
                        }

                    }
                }
            }
            #endregion

            #endregion

            #region Derinlik Eşitlemesiz Kodlar
            //for (int i = 1; i < rowCount; i++)
            //{
            //    sisItem = new SismikDTO();
            //    sisItem.ID = i;
            //    sisItem.Adi = (string)(xlWorksheetSismik.Cells[i + 1, 1]).Value.ToString();
            //    sisItem.X = (double)(xlWorksheetSismik.Cells[i + 1, 2]).Value;
            //    sisItem.K = (double)(xlWorksheetSismik.Cells[i + 1, 4]).Value;
            //    sisList.Add(sisItem);
            //}
            //sisGenelList.Add(sisList);

            //for (int j = 5; j <= colCount; j = j + 3)
            //{
            //    sisList = new List<SismikDTO>();
            //    for (int i = 1; i <= rowCount; i++)
            //    {

            //        sisItem = new SismikDTO();
            //        if ((xlWorksheetSismik.Cells[i + 1, j]).Value == null && (xlWorksheetSismik.Cells[i + 1, j + 1]).Value == null && (xlWorksheetSismik.Cells[i + 1, j + 2]).Value == null)
            //        {
            //            continue;
            //        }
            //        if ((xlWorksheetSismik.Cells[i + 1, j]).Value == null && (xlWorksheetSismik.Cells[i + 1, j + 1]).Value != null && (xlWorksheetSismik.Cells[i + 1, j + 2]).Value != null)
            //        {
            //            sisItem.ID = i;
            //            sisItem.Adi = (string)(xlWorksheetSismik.Cells[i + 1, 1]).Value.ToString() + count.ToString();
            //            sisItem.X = (double)(xlWorksheetSismik.Cells[i + 1, 2]).Value;
            //            sisItem.K = ((double)(xlWorksheetSismik.Cells[i + 1, 4]).Value - (double)(xlWorksheetSismik.Cells[i + 1, j - 3]).Value) * 0.99;
            //            sisItem.Vp = (xlWorksheetSismik.Cells[i + 1, j + 1]).Value == null ? 0 : (double)(xlWorksheetSismik.Cells[i + 1, j + 1]).Value;
            //            sisItem.Vs = (xlWorksheetSismik.Cells[i + 1, j + 2]).Value == null ? 0 : (double)(xlWorksheetSismik.Cells[i + 1, j + 2]).Value;
            //            sisList.Add(sisItem);
            //            count++;
            //            continue;
            //        }
            //        sisItem.ID = i;
            //        sisItem.Adi = (string)(xlWorksheetSismik.Cells[i + 1, 1]).Value.ToString() + count.ToString();
            //        sisItem.X = (double)(xlWorksheetSismik.Cells[i + 1, 2]).Value;
            //        sisItem.K = (xlWorksheetSismik.Cells[i + 1, j]).Value == null ? 0 : (double)(xlWorksheetSismik.Cells[i + 1, 4]).Value - (double)(xlWorksheetSismik.Cells[i + 1, j]).Value;
            //        sisItem.Vp = (xlWorksheetSismik.Cells[i + 1, j + 1]).Value == null ? 0 : (double)(xlWorksheetSismik.Cells[i + 1, j + 1]).Value;
            //        sisItem.Vs = (xlWorksheetSismik.Cells[i + 1, j + 2]).Value == null ? 0 : (double)(xlWorksheetSismik.Cells[i + 1, j + 2]).Value;
            //        sisList.Add(sisItem);
            //        count++;
            //    }
            //    sisGenelList.Add(sisList);
            //    count = 1;
            //} 
            #endregion

            for (int i = 0; i < rowCount - 1; i++)
            {
                sisItem = new SeismicDTO();
                sisItem.ID = i + 1;
                sisItem.Name = sisExcel[i][0].Value.ToString();
                sisItem.X = Convert.ToDouble(sisExcel[i][1].Value);
                sisItem.K = Convert.ToDouble(sisExcel[i][3].Value);
                sisList.Add(sisItem);
            }
            sisGenelList.Add(sisList);

            int count = 0;
            for (int j = 4; j < colCount; j = j + 3)
            {
                count++;
                sisList = new List<SeismicDTO>();
                for (int i = 0; i < rowCount - 1; i++)
                {
                    var sisExcelInstance = sisExcel[i];
                    if (sisExcelInstance[j].TypeID == (byte)Enums.ExcelDataType.Real)
                    {
                        if (sisExcelInstance[j].Value == "" && sisExcelInstance[j + 1].Value == "" && sisExcelInstance[j + 2].Value == "")
                        {
                            continue;
                        }
                        if (sisExcelInstance[j].Value == "" && sisExcelInstance[j + 1].Value != "" && sisExcelInstance[j + 2].Value != "")
                        {
                            sisItem = new SeismicDTO();
                            sisItem.ID = i + 1;
                            sisItem.Name = sisExcelInstance[0].Value.ToString() + count.ToString();
                            sisItem.X = Convert.ToDouble(sisExcelInstance[1].Value);
                            var value = "";
                            for (int k = 0; k < sisExcelInstance.Count; k = k + 3)
                            {
                                if (sisExcelInstance[j - (3 + k)].TypeID == (byte)Enums.ExcelDataType.Real)
                                {
                                    value = sisExcelInstance[j - (3 + k)].Value;
                                    break;
                                }
                            }
                            sisItem.K = (Convert.ToDouble(sisExcelInstance[3].Value) - Convert.ToDouble(value)) * 0.99;
                            sisItem.Vp = sisExcelInstance[j + 1].Value == "" ? Convert.ToDouble("") : Convert.ToDouble(sisExcelInstance[j + 1].Value);
                            sisItem.Vs = sisExcelInstance[j + 2].Value == "" ? Convert.ToDouble("") : Convert.ToDouble(sisExcelInstance[j + 2].Value);
                            sisList.Add(sisItem);
                            continue;
                        }
                        sisItem = new SeismicDTO();
                        sisItem.ID = i + 1;
                        sisItem.Name = sisExcelInstance[0].Value.ToString() + count.ToString();
                        sisItem.X = Convert.ToDouble(sisExcelInstance[1].Value);
                        sisItem.K = sisExcelInstance[j].Value == "" ? 0 : Convert.ToDouble(sisExcelInstance[3].Value) - Convert.ToDouble(sisExcelInstance[j].Value);
                        sisItem.Vp = sisExcelInstance[j + 1].Value == "" ? Convert.ToDouble("") : Convert.ToDouble(sisExcelInstance[j + 1].Value);
                        sisItem.Vs = sisExcelInstance[j + 2].Value == "" ? Convert.ToDouble("") : Convert.ToDouble(sisExcelInstance[j + 2].Value);
                        sisList.Add(sisItem);
                    }
                }
                sisGenelList.Add(sisList);
            }

            highcharts = ChartOlustur(highcharts, sisGenelList);
        }
        private void SondajOlustur(HighchartsDTO highcharts, Workbook xlWorkbook)
        {
            sonGenelList = new List<List<DrillDTO>>();
            List<DrillDTO> sonList = new List<DrillDTO>();
            DrillDTO sonItem = new DrillDTO();
            Microsoft.Office.Interop.Excel._Worksheet xlWorkSheetSondaj = (Microsoft.Office.Interop.Excel._Worksheet)xlWorkbook.Sheets[3];
            Microsoft.Office.Interop.Excel.Range xlRange = xlWorkSheetSondaj.UsedRange;
            #region Tablo Satir ve Sutun Genislikleri
            int rowCount = xlRange.Rows.Count;
            int colCount = xlRange.Columns.Count;
            for (int i = 1; i <= rowCount; i++)
            {
                if (string.IsNullOrEmpty((xlWorkSheetSondaj.Cells[i + 1, 1]).Value))
                {
                    rowCount = i;
                    break;
                }
            }
            #endregion

            for (int i = 1; i < rowCount; i++)
            {
                sonItem = new DrillDTO();
                sonItem.ID = i;
                sonItem.Name = (string)(xlWorkSheetSondaj.Cells[i + 1, 1]).Value.ToString();
                sonItem.X = (double)(xlWorkSheetSondaj.Cells[i + 1, 2]).Value;
                sonItem.K = (double)(xlWorkSheetSondaj.Cells[i + 1, 4]).Value;
                sonList.Add(sonItem);
            }
            sonGenelList.Add(sonList);

            int count = 0;
            for (int j = 5; j <= colCount; j = j + 2)
            {
                count++;
                sonList = new List<DrillDTO>();
                for (int i = 1; i <= rowCount; i++)
                {

                    sonItem = new DrillDTO();
                    if ((xlWorkSheetSondaj.Cells[i + 1, j]).Value == null && (xlWorkSheetSondaj.Cells[i + 1, j + 1]).Value == null)
                    {
                        continue;
                    }
                    if ((xlWorkSheetSondaj.Cells[i + 1, j]).Value == null && (xlWorkSheetSondaj.Cells[i + 1, j + 1]).Value != null)
                    {
                        sonItem.ID = i;
                        sonItem.Name = (string)(xlWorkSheetSondaj.Cells[i + 1, 1]).Value.ToString() + count.ToString();
                        sonItem.X = (double)(xlWorkSheetSondaj.Cells[i + 1, 2]).Value;
                        sonItem.K = ((double)(xlWorkSheetSondaj.Cells[i + 1, 4]).Value - (double)(xlWorkSheetSondaj.Cells[i + 1, j - 2]).Value) * 0.99;
                        sonItem.T = (xlWorkSheetSondaj.Cells[i + 1, j + 1]).Value == null ? "" : (xlWorkSheetSondaj.Cells[i + 1, j + 1]).Value;
                        sonList.Add(sonItem);
                        continue;
                    }
                    sonItem.ID = i;
                    sonItem.Name = (string)(xlWorkSheetSondaj.Cells[i + 1, 1]).Value.ToString() + count.ToString();
                    sonItem.X = (double)(xlWorkSheetSondaj.Cells[i + 1, 2]).Value;
                    sonItem.K = (xlWorkSheetSondaj.Cells[i + 1, j]).Value == null ? 0 : (double)(xlWorkSheetSondaj.Cells[i + 1, 4]).Value - (double)(xlWorkSheetSondaj.Cells[i + 1, j]).Value;
                    sonItem.T = (xlWorkSheetSondaj.Cells[i + 1, j + 1]).Value == null ? "" : ((xlWorkSheetSondaj.Cells[i + 1, j + 1]).Value).ToString();
                    sonList.Add(sonItem);
                }
                sonGenelList.Add(sonList);
            }

            highcharts = ChartOlustur(highcharts, sonGenelList);
        }

        private HighchartsDTO ChartOlustur(HighchartsDTO highcharts, List<List<ResistivityDTO>> rezGenelList)
        {
            //highcharts.series.AddRange(GraphDataOlustur(rezGenelList));
            highcharts.annotations.AddRange(GraphAnnotationsOlustur(rezGenelList));

            return highcharts;
        }
        private HighchartsDTO ChartOlustur(HighchartsDTO highcharts, List<List<SeismicDTO>> sisGenelList)
        {
            //highcharts.series.AddRange(GraphDataOlustur(sisGenelList));
            highcharts.annotations.AddRange(GraphAnnotationsOlustur(sisGenelList));

            return highcharts;
        }
        private HighchartsDTO ChartOlustur(HighchartsDTO highcharts, List<List<DrillDTO>> sonGenelList)
        {
            //highcharts.series.AddRange(GraphDataOlustur(sonGenelList));
            highcharts.annotations.AddRange(GraphAnnotationsOlustur(sonGenelList));

            return highcharts;
        }

        private List<AnnotationsDTO> GraphAnnotationsOlustur(List<List<ResistivityDTO>> rezGenelList)
        {
            List<AnnotationsDTO> annotationsList = new List<AnnotationsDTO>();
            AnnotationsDTO annotations;
            AnnotationLabelsDTO label;

            for (int i = 0; i < rezGenelList.Count; i++)
            {
                annotations = new AnnotationsDTO();
                annotations.visible = true;
                //annotations.labelOptions = new AnnotationLabelOptionsDTO { shape = "connector", align = "right", justify = false, crop = true, style = new StyleDTO { fontSize = "0.8em", textOutline = "1px white" } };
                foreach (var rezItem in rezGenelList[i].Where(k => k.TypeID == (byte)Enums.ExcelDataType.Real))
                {
                    if (i == 0)
                        label = new AnnotationLabelsDTO { x = -20, y = -20, point = new PointDTO { xAxis = 0, yAxis = 0, x = rezItem.X, y = rezItem.K }, text = rezItem.Name, shape = "connector", allowOverlap = true };
                    //label = new AnnotationLabelsDTO { x = -20, y = -20, point = new PointDTO { xAxis = 0, yAxis = 0, x = rezItem.X, y = rezItem.K }, text = rezItem.Adi + "<br>X:" + rezItem.X + " Y:" + rezItem.K, shape = "connector", allowOverlap = true };
                    else
                        label = new AnnotationLabelsDTO { x = -20, y = -20, point = new PointDTO { xAxis = 0, yAxis = 0, x = rezItem.X, y = rezItem.K }, text = rezItem.R + " ohm", shape = "connector", allowOverlap = true };
                    //label = new AnnotationLabelsDTO { x = -20, y = -20, point = new PointDTO { xAxis = 0, yAxis = 0, x = rezItem.X, y = rezItem.K }, text = rezItem.R + " ohm<br>X:" + rezItem.X + " Y:" + rezItem.K, shape = "connector", allowOverlap = true };
                    annotations.labels.Add(label);
                }
                annotationsList.Add(annotations);
            }

            return annotationsList;
        }
        private List<AnnotationsDTO> GraphAnnotationsOlustur(List<List<SeismicDTO>> sisGenelList)
        {
            List<AnnotationsDTO> annotationsList = new List<AnnotationsDTO>();
            AnnotationsDTO annotations;
            AnnotationLabelsDTO label;

            for (int i = 0; i < sisGenelList.Count; i++)
            {
                annotations = new AnnotationsDTO();
                annotations.visible = true;
                //annotations.labelOptions = new AnnotationLabelOptionsDTO { shape = "connector", align = "right", justify = false, crop = true, style = new StyleDTO { fontSize = "0.8em", textOutline = "1px white" } };
                foreach (var sisItem in sisGenelList[i])
                {
                    if (i == 0)
                        label = new AnnotationLabelsDTO { x = 20, y = -20, point = new PointDTO { xAxis = 0, yAxis = 0, x = sisItem.X, y = sisItem.K }, text = sisItem.Name, shape = "connector", allowOverlap = true };
                    //label = new AnnotationLabelsDTO { x = 20, y = -20, point = new PointDTO { xAxis = 0, yAxis = 0, x = sisItem.X, y = sisItem.K }, text = sisItem.Adi + "<br>X:" + sisItem.X + " Y:" + sisItem.K, shape = "connector", allowOverlap = true };
                    else
                        label = new AnnotationLabelsDTO { x = 20, y = -20, point = new PointDTO { xAxis = 0, yAxis = 0, x = sisItem.X, y = sisItem.K }, text = "Vp = " + sisItem.Vp + "m/s<br>Vs =" + sisItem.Vs + "m/s", shape = "connector", allowOverlap = true };
                    //label = new AnnotationLabelsDTO { x = 20, y = -20, point = new PointDTO { xAxis = 0, yAxis = 0, x = sisItem.X, y = sisItem.K }, text = "Vp = " + sisItem.Vp + "m/s<br>Vs =" + sisItem.Vs + "m/s<br>X:" + sisItem.X + " Y:" + sisItem.K, shape = "connector", allowOverlap = true };
                    annotations.labels.Add(label);
                }
                annotationsList.Add(annotations);
            }

            return annotationsList;
        }
        private List<AnnotationsDTO> GraphAnnotationsOlustur(List<List<DrillDTO>> sonGenelList)
        {
            List<AnnotationsDTO> annotationsList = new List<AnnotationsDTO>();
            AnnotationsDTO annotations;
            AnnotationLabelsDTO label;

            for (int i = 0; i < sonGenelList.Count; i++)
            {
                annotations = new AnnotationsDTO();
                annotations.visible = true;
                // annotations.labelOptions = new AnnotationLabelOptionsDTO { shape = "connector", align = "right", justify = false, crop = true, style = new StyleDTO { fontSize = "0.8em", textOutline = "1px white" } };
                foreach (var sonItem in sonGenelList[i])
                {
                    if (i == 0)
                        //label = new AnnotationLabelsDTO { x = 20, y = 20, point = new PointDTO { xAxis = 0, yAxis = 0, x = sonItem.X, y = sonItem.K }, text = sonItem.Adi + "<br>X:" + sonItem.X + " Y:" + sonItem.K, shape = "connector", allowOverlap = true };
                        label = new AnnotationLabelsDTO { x = 20, y = 20, point = new PointDTO { xAxis = 0, yAxis = 0, x = sonItem.X, y = sonItem.K }, text = sonItem.Name, shape = "connector", allowOverlap = true };
                    else
                        //label = new AnnotationLabelsDTO { x = 20, y = 20, point = new PointDTO { xAxis = 0, yAxis = 0, x = sonItem.X, y = sonItem.K }, text = sonItem.T + "<br>X:" + sonItem.X + " Y:" + sonItem.K, shape = "connector", allowOverlap = true };
                        label = new AnnotationLabelsDTO { x = 20, y = 20, point = new PointDTO { xAxis = 0, yAxis = 0, x = sonItem.X, y = sonItem.K }, text = sonItem.T, shape = "connector", allowOverlap = true };
                    annotations.labels.Add(label);
                }
                annotationsList.Add(annotations);
            }

            return annotationsList;
        }

        public List<SeriesDTO> GraphDataOlustur(long kuralID, SectionDTO kesitDTO, ParametersDTO parameters)
        {
            GetRuleDTO kuralGetir = _fuzzyManager.KuralGetir(kuralID);
            GraphDetailedDTO cizimDetailed = new GraphDetailedDTO();

            SeriesDTO dataset;
            var name = "Set-";
            int count = 0;
            var random = new Random();
            for (int i = 0; i < kesitDTO.ResistivityGeneralList.Count - 1; i++)
            {
                count++;
                dataset = new SeriesDTO();
                dataset.name = name + count.ToString();
                if ((bool)parameters.IsGraphsVisible)
                    dataset.lineWidth = 0;
				dataset.lineWidth = 2;
				dataset.color = RenkUret(i, kesitDTO.ResistivityGeneralList.Count); // String.Format("#{0:X6}", random.Next(0x1000000));
                dataset.showInLegend = false;
                dataset.marker = new MarkerDTO { symbol = "circle", radius = 2, enabled = true };
                dataset.tooltip = new ToolTipDTO
                {
                    useHTML = true,
                    headerFormat = "<small>{series.name}</small><table style='color: {series.color}'><br />",
                    pointFormat = "<tr><td style='text-align: right'><b>{point.x}, {point.y}</b></td></tr>",
                    footerFormat = "</table>",
                    valueDecimals = 2
                };
                dataset.states = new StatesDTO { hover = new HoverDTO { lineWidthPlus = 3 } };
                //dataset.enableMouseTracking = false;
                dataset.draggableY = true;
                dataset.draggableX = true;
                for (int j = 0; j < kesitDTO.ResistivityGeneralList[i].Count; j++)
                {
                    List<double> coordinates = new List<double>();

                    if (i == 0 && j == 0 && !kesitDTO.ResistivityGeneralList[i][j].Checked)//İlk Sol Sismik Çizimi
                    {
                        CizimeSismikEkle(kesitDTO.ResistivityGeneralList[i][j].X, kesitDTO.SeismicGeneralList[i], kesitDTO.ResistivityGeneralList[i], dataset, (byte)Enums.DirectionValue.Left, j);
                    }

                    #region Topografya (İlk Çizgi) Çizimi Koşulsuz Yapılmalı 
                    if (i == 0)
                    {
                        if (!kesitDTO.ResistivityGeneralList[i][j].Checked)
                        {
                            coordinates.Add(kesitDTO.ResistivityGeneralList[i][j].X);
                            coordinates.Add((double)kesitDTO.ResistivityGeneralList[i][j].K);
                            dataset.data.Add(coordinates);
                            kesitDTO.ResistivityGeneralList[i][j].Checked = true;

                            if (j + 1 < kesitDTO.ResistivityGeneralList[i].Count)
                            {
                                cizimDetailed = new GraphDetailedDTO { FirstNode = kesitDTO.ResistivityGeneralList[i][j].Name, SecondNode = kesitDTO.ResistivityGeneralList[i][j + 1].Name, Normal = true, Connection = "Normal" };
                                cizimDetailedList.Add(cizimDetailed);
                            }
                            cizimCount.Normal++;

                            if (j == kesitDTO.ResistivityGeneralList[i].Count - 1)
                            {
                                CizimeSismikEkle(kesitDTO.ResistivityGeneralList[i][j].X, kesitDTO.SeismicGeneralList[i], kesitDTO.ResistivityGeneralList[i], dataset, (byte)Enums.DirectionValue.Right, j);
                            }
                        }
                        continue;
                    }
                    #endregion

                    NodeDTO uygunIlkDugum = new NodeDTO();
                    NodeDTO uygunIkinciDugum = new NodeDTO();
                    if (j != kesitDTO.ResistivityGeneralList[i].Count - 1) //Son sıra kontrolü
                    {

                        uygunIlkDugum = UygunIlkDugumKontrolu(kesitDTO.ResistivityGeneralList, i, j);
                        uygunIkinciDugum = UygunIkinciDugumKontrolu(kuralGetir, kesitDTO, kesitDTO.ResistivityGeneralList, i, j + 1, parameters);
                        if ((uygunIlkDugum.Node.TypeID == (byte)Enums.ExcelDataType.Real && uygunIkinciDugum.Node.TypeID == (byte)Enums.ExcelDataType.Real) ||
                            (uygunIlkDugum.Node.TypeID == (byte)Enums.ExcelDataType.Artificial && uygunIkinciDugum.Node.TypeID == (byte)Enums.ExcelDataType.Real) ||
                            (uygunIlkDugum.Node.TypeID == (byte)Enums.ExcelDataType.Real && uygunIkinciDugum.Node.TypeID == (byte)Enums.ExcelDataType.Artificial))
                        {
                            if ((!uygunIlkDugum.Node.Checked && !uygunIkinciDugum.Node.Checked) ||
                                (uygunIlkDugum.Node.Checked && !uygunIkinciDugum.Node.Checked) ||
                                (!uygunIlkDugum.Node.Checked && uygunIkinciDugum.Node.Checked))
                            {
                                if (uygunIlkDugum.Node.R != null && uygunIkinciDugum.Node.R != null && uygunIlkDugum.Node.R != 0 && uygunIkinciDugum.Node.R != 0)
                                {
                                    //if (!kesitDTO.RezGenelList[i][j].Checked && kesitDTO.RezGenelList[i][j].TypeID == (byte)Enums.ExcelDataTipi.Gercek)
                                    //{
                                    //var ilkDugum = _fuzzyManager.FuzzyKuralOlusturVeSonucGetirFLL(kuralGetir, (double)kesitDTO.RezGenelList[i][j].R);
                                    //var ikinciDugum = _fuzzyManager.FuzzyKuralOlusturVeSonucGetirFLL(kuralGetir, (double)kesitDTO.RezGenelList[i][j + 1].R);

                                    var ikiDugumKarsilastirma = _fuzzyManager.FuzzyKuralOlusturVeSonucGetirFLLKarsilastirma(kuralGetir, (double)uygunIlkDugum.Node.R, (double)uygunIkinciDugum.Node.R, (int)parameters.ResistivityRatio);

                                    //if (ilkDugum == ikinciDugum) //iki özdirenç değeri de aynı aralıktaysa bu sefer hız değerlerine bakılır
                                    if (ikiDugumKarsilastirma) //iki özdirenç değeri de aynı aralıktaysa bu sefer hız değerlerine bakılır
                                    {
                                        bool VpUygunMu = SismikKontroluVp(kesitDTO, uygunIlkDugum.IndexI, uygunIlkDugum.IndexJ, (int)parameters.SeismicRatio);
                                        bool VsUygunMu = SismikKontroluVs(kesitDTO, uygunIlkDugum.IndexI, uygunIlkDugum.IndexJ, (int)parameters.SeismicRatio);
                                        if (VpUygunMu && VsUygunMu) //Vp Vs ve Özdirenç değerleri uygunsa birleştirme yapılır
                                        {
                                            #region Özdirenç Değerinin Solunda Olan Sismik Değerlerinin Kontrolü
                                            if (j == 0)
                                            //if (j == 0 && kesitDTO.SisGenelList.Count >= kesitDTO.RezGenelList.Count)
                                            {
                                                if (!kesitDTO.ResistivityGeneralList[i][j].Checked && i < kesitDTO.SeismicGeneralList.Count)
                                                {
                                                    CizimeSismikEkle(kesitDTO.ResistivityGeneralList[i][j].X, kesitDTO.SeismicGeneralList[i], kesitDTO.ResistivityGeneralList[i], dataset, (byte)Enums.DirectionValue.Left, j);
                                                }
                                            }
                                            #endregion

                                            var IlkDugumunSolundaFay = IlkDugumunSolundaFayKontrolu(kesitDTO, uygunIlkDugum, uygunIkinciDugum, datasetList);
                                            if (IlkDugumunSolundaFay != null)
                                            {
                                                List<double> coordinatesNull = new List<double>();
                                                dataset.data.Add(coordinatesNull);

                                                List<double> coordinatesSolFay = new List<double>();
                                                coordinatesSolFay.Add(IlkDugumunSolundaFay.data[0][0]);
                                                coordinatesSolFay.Add((double)uygunIlkDugum.Node.K);
                                                dataset.data.Add(coordinatesSolFay);

                                                cizimDetailed = new GraphDetailedDTO { FirstNode = "Fay", SecondNode = uygunIkinciDugum.Node.Name, Normal = true, Connection = "Normal" };
                                                cizimDetailedList.Add(cizimDetailed);
                                                cizimCount.Normal++;
                                            }
                                            if (!kesitDTO.ResistivityGeneralList[uygunIlkDugum.IndexI][uygunIlkDugum.IndexJ].Checked)
                                            {
                                                coordinates = new List<double>();
                                                coordinates.Add(uygunIlkDugum.Node.X);
                                                coordinates.Add((double)uygunIlkDugum.Node.K);
                                                dataset.data.Add(coordinates);
                                                kesitDTO.ResistivityGeneralList[uygunIlkDugum.IndexI][uygunIlkDugum.IndexJ].Checked = true;
                                            }
                                            var IlkDugumunSagindaFay = IlkDugumunSagindaFayKontrolu(kesitDTO, uygunIlkDugum, uygunIkinciDugum, datasetList);
                                            if (IlkDugumunSagindaFay != null)
                                            {
                                                List<double> coordinatesSagFay = new List<double>();
                                                coordinatesSagFay.Add(IlkDugumunSagindaFay.data[0][0]);
                                                coordinatesSagFay.Add((double)uygunIlkDugum.Node.K);
                                                dataset.data.Add(coordinatesSagFay);

                                                cizimDetailed = new GraphDetailedDTO { FirstNode = uygunIlkDugum.Node.Name, SecondNode = "Fay", Normal = true, Connection = "Normal" };
                                                cizimDetailedList.Add(cizimDetailed);
                                                cizimCount.Normal++;
                                                continue;
                                            }
                                            coordinates = new List<double>();
                                            coordinates.Add(uygunIkinciDugum.Node.X);
                                            coordinates.Add((double)uygunIkinciDugum.Node.K);
                                            dataset.data.Add(coordinates);
                                            kesitDTO.ResistivityGeneralList[uygunIkinciDugum.IndexI][uygunIkinciDugum.IndexJ].Checked = true;

                                            cizimDetailed = new GraphDetailedDTO { FirstNode = uygunIlkDugum.Node.Name, SecondNode = uygunIkinciDugum.Node.Name, Normal = true, Connection = "Normal" };
                                            cizimDetailedList.Add(cizimDetailed);
                                            cizimCount.Normal++;
                                        }
                                        else //özdirenç değerleri uygun ama sismik değerleri değil. çukur ve fay kontrolü yapılır
                                        {
                                            if (j == 0) //en üst düzey kontrolü
                                            {
                                                //Fay oluştur
                                                bool KapatmaCizilebilirMi = KapatmaKontrolu(datasetList, dataset, kesitDTO.ResistivityGeneralList, uygunIlkDugum.IndexI, uygunIkinciDugum.IndexJ);
                                                //Çukur oluştur
                                                //if (KapatmaCizilebilirMi)
                                                //{
                                                coordinates.Add(uygunIlkDugum.Node.X);
                                                coordinates.Add((double)uygunIlkDugum.Node.K);
                                                dataset.data.Add(coordinates);
                                                kesitDTO.ResistivityGeneralList[uygunIlkDugum.IndexI][uygunIlkDugum.IndexJ].Checked = true;
                                                KapatmaOlustur(datasetList, dataset, kesitDTO.ResistivityGeneralList, uygunIlkDugum.IndexI, uygunIlkDugum.IndexJ);

                                                cizimDetailed = new GraphDetailedDTO { FirstNode = uygunIlkDugum.Node.Name, SecondNode = uygunIkinciDugum.Node.Name, Closure = true, Connection = "Kapatma" };
                                                cizimDetailedList.Add(cizimDetailed);
                                                cizimCount.Closure++;
                                                //}
                                                break;
                                            }
                                            else
                                            {
                                                var birOncekiDugum = _fuzzyManager.FuzzyKuralOlusturVeSonucGetirFLL(kuralGetir, (double)kesitDTO.ResistivityGeneralList[uygunIlkDugum.IndexI][uygunIlkDugum.IndexJ - 1].R);
                                                var cukurKarsilastirma = _fuzzyManager.FuzzyKuralOlusturVeSonucGetirFLLKarsilastirma(kuralGetir, (double)kesitDTO.ResistivityGeneralList[uygunIlkDugum.IndexI][uygunIlkDugum.IndexJ - 1].R, (double)uygunIkinciDugum.Node.R, (int)parameters.ResistivityRatio);
                                                //if (cukurKarsilastirma)
                                                //{
                                                //    //Çukur oluştur
                                                //    var cukurDataset = CukurOlustur(dataset, kesitDTO.RezGenelList, uygunIlkDugum.IndexI, uygunIlkDugum.IndexJ);
                                                //    datasetList.Add(cukurDataset);
                                                //    if (i < kesitDTO.RezGenelList.Count - 1)
                                                //        cizimCount.Kapatma++;
                                                //    continue;
                                                //}
                                                //else
                                                //{
                                                //Fay oluştur
                                                bool KapatmaCizilebilirMi = KapatmaKontrolu(datasetList, dataset, kesitDTO.ResistivityGeneralList, uygunIlkDugum.IndexI, uygunIkinciDugum.IndexJ);
                                                //Çukur oluştur
                                                //if (KapatmaCizilebilirMi)
                                                // {
                                                coordinates.Add(uygunIlkDugum.Node.X);
                                                coordinates.Add((double)uygunIlkDugum.Node.K);
                                                dataset.data.Add(coordinates);
                                                kesitDTO.ResistivityGeneralList[uygunIlkDugum.IndexI][uygunIlkDugum.IndexJ].Checked = true;
                                                KapatmaOlustur(datasetList, dataset, kesitDTO.ResistivityGeneralList, uygunIlkDugum.IndexI, uygunIkinciDugum.IndexJ);

                                                cizimDetailed = new GraphDetailedDTO { FirstNode = uygunIlkDugum.Node.Name, SecondNode = uygunIkinciDugum.Node.Name, Closure = true, Connection = "Kapatma" };
                                                cizimDetailedList.Add(cizimDetailed);
                                                cizimCount.Closure++;
                                                //}
                                                break;
                                                //}
                                            }
                                        }
                                    }
                                    else //özdirenç değerleri uygun değil. fay ve kapatma kontrolü yapılır
                                    {
                                        if (j == 0)
                                        {
                                            bool KapatmaCizilebilirMi = KapatmaKontrolu(datasetList, dataset, kesitDTO.ResistivityGeneralList, uygunIlkDugum.IndexI, uygunIkinciDugum.IndexJ);
                                            //Çukur oluştur
                                            // if (KapatmaCizilebilirMi)
                                            //{
                                            coordinates.Add(uygunIlkDugum.Node.X);
                                            coordinates.Add((double)uygunIlkDugum.Node.K);
                                            dataset.data.Add(coordinates);
                                            kesitDTO.ResistivityGeneralList[uygunIlkDugum.IndexI][uygunIlkDugum.IndexJ].Checked = true;
                                            KapatmaOlustur(datasetList, dataset, kesitDTO.ResistivityGeneralList, uygunIlkDugum.IndexI, uygunIkinciDugum.IndexJ);

                                            cizimDetailed = new GraphDetailedDTO { FirstNode = uygunIlkDugum.Node.Name, SecondNode = uygunIkinciDugum.Node.Name, Closure = true, Connection = "Kapatma" };
                                            cizimDetailedList.Add(cizimDetailed);
                                            cizimCount.Closure++;
                                            //}
                                            break;
                                        }
                                        else
                                        {
                                            var fayKontrolü = FayKontrolu(kuralGetir, kesitDTO, uygunIlkDugum, uygunIkinciDugum, parameters);
                                            //Fay oluştur
                                            if (fayKontrolü && i > 1)
                                            {
                                                if (!kesitDTO.ResistivityGeneralList[uygunIlkDugum.IndexI][uygunIlkDugum.IndexJ].Checked)
                                                {
                                                    //Buraya hesaplama şeysi eklenebilir mi
                                                    coordinates.Add(uygunIlkDugum.Node.X);
                                                    coordinates.Add((double)uygunIlkDugum.Node.K);
                                                    dataset.data.Add(coordinates);
                                                    kesitDTO.ResistivityGeneralList[uygunIlkDugum.IndexI][uygunIlkDugum.IndexJ].Checked = true;
                                                }
                                                //Fay oluşturma kodları

                                                SeriesDTO fayDataset = FayOlustur(kuralGetir, kesitDTO, uygunIlkDugum, uygunIkinciDugum, parameters);
                                                datasetList.Add(fayDataset);

                                                if (!(kesitDTO.ResistivityGeneralList[uygunIlkDugum.IndexI][uygunIlkDugum.IndexJ].TypeID == (byte)Enums.ExcelDataType.Artificial && kesitDTO.ResistivityGeneralList[uygunIlkDugum.IndexI][uygunIlkDugum.IndexJ - 1].TypeID == (byte)Enums.ExcelDataType.Artificial))
                                                {
                                                    List<double> fayCoordinates = new List<double>();
                                                    fayCoordinates.Add(fayDataset.data[0][0]);
                                                    fayCoordinates.Add((double)uygunIlkDugum.Node.K);
                                                    dataset.data.Add(fayCoordinates);
                                                }

                                                cizimDetailed = new GraphDetailedDTO { FirstNode = uygunIlkDugum.Node.Name, SecondNode = uygunIkinciDugum.Node.Name, Fault = true, Connection = "Fay" };
                                                cizimDetailedList.Add(cizimDetailed);
                                                cizimCount.Fault++;

                                                continue;
                                            }
                                            else
                                            {

                                                var birOncekiDugum = _fuzzyManager.FuzzyKuralOlusturVeSonucGetirFLL(kuralGetir, (double)kesitDTO.ResistivityGeneralList[uygunIlkDugum.IndexI][uygunIlkDugum.IndexJ - 1].R);
                                                var cukurKarsilastirma = _fuzzyManager.FuzzyKuralOlusturVeSonucGetirFLLKarsilastirma(kuralGetir, (double)kesitDTO.ResistivityGeneralList[uygunIlkDugum.IndexI][uygunIlkDugum.IndexJ - 1].R, (double)uygunIkinciDugum.Node.R, (int)parameters.ResistivityRatio);

                                                //if (birOncekiDugum == ikinciDugum)
                                                //if (cukurKarsilastirma)
                                                //{
                                                //    bool KapatmaCizilebilirMi = KapatmaKontrolu(datasetList, dataset, kesitDTO.RezGenelList, uygunIlkDugum.IndexI, uygunIkinciDugum.IndexJ);
                                                //    //Çukur oluştur
                                                //    if (KapatmaCizilebilirMi)
                                                //    {
                                                //        coordinates.Add(uygunIlkDugum.Dugum.X);
                                                //        coordinates.Add((double)uygunIlkDugum.Dugum.K);
                                                //        dataset.data.Add(coordinates);
                                                //        kesitDTO.RezGenelList[uygunIlkDugum.IndexI][uygunIlkDugum.IndexJ].Checked = true;
                                                //        KapatmaOlustur(datasetList, dataset, kesitDTO.RezGenelList, uygunIlkDugum.IndexI, uygunIkinciDugum.IndexJ);
                                                //        if (i < kesitDTO.RezGenelList.Count - 1)
                                                //            cizimCount.Kapatma++;
                                                //    }
                                                //    break;
                                                //}
                                                //else
                                                //{
                                                bool KapatmaCizilebilirMi = KapatmaKontrolu(datasetList, dataset, kesitDTO.ResistivityGeneralList, uygunIlkDugum.IndexI, uygunIkinciDugum.IndexJ);
                                                //Fay oluştur
                                                if (KapatmaCizilebilirMi)
                                                {
                                                    if (!kesitDTO.ResistivityGeneralList[uygunIlkDugum.IndexI][uygunIlkDugum.IndexJ].Checked)
                                                    {
                                                        coordinates.Add(uygunIlkDugum.Node.X);
                                                        coordinates.Add((double)uygunIlkDugum.Node.K);
                                                        dataset.data.Add(coordinates);
                                                        kesitDTO.ResistivityGeneralList[uygunIlkDugum.IndexI][uygunIlkDugum.IndexJ].Checked = true;
                                                    }

                                                    KapatmaOlustur(datasetList, dataset, kesitDTO.ResistivityGeneralList, uygunIlkDugum.IndexI, uygunIkinciDugum.IndexJ);

                                                    cizimDetailed = new GraphDetailedDTO { FirstNode = uygunIlkDugum.Node.Name, SecondNode = uygunIkinciDugum.Node.Name, Closure = true, Connection = "Kapatma" };
                                                    cizimDetailedList.Add(cizimDetailed);
                                                    cizimCount.Closure++;
                                                    break;
                                                }
                                                var IlkDugumunSagindaFay = IlkDugumunSagindaFayKontrolu(kesitDTO, uygunIlkDugum, uygunIkinciDugum, datasetList);
                                                if (IlkDugumunSagindaFay != null)
                                                {
                                                    List<double> coordinatesSagFay = new List<double>();
                                                    coordinatesSagFay.Add(IlkDugumunSagindaFay.data[0][0]);
                                                    coordinatesSagFay.Add((double)uygunIlkDugum.Node.K);
                                                    dataset.data.Add(coordinatesSagFay);

                                                    cizimDetailed = new GraphDetailedDTO { FirstNode = uygunIlkDugum.Node.Name, SecondNode = "Fay", Normal = true, Connection = "Normal" };
                                                    cizimDetailedList.Add(cizimDetailed);
                                                    cizimCount.Normal++;
                                                    continue;
                                                }
                                                //}
                                            }
                                        }
                                    }
                                    //}
                                }
                            }
                        }
                        else
                        {
                            kesitDTO.ResistivityGeneralList[uygunIlkDugum.IndexI][uygunIlkDugum.IndexJ].Checked = true;
                            kesitDTO.ResistivityGeneralList[uygunIkinciDugum.IndexI][uygunIkinciDugum.IndexJ].Checked = true;
                            continue;
                        }
                    }
                    else
                    {
                        //coordinates.Add(kesitDTO.RezGenelList[i][j].X);
                        //coordinates.Add((double)kesitDTO.RezGenelList[i][j].K);
                        //dataset.data.Add(coordinates);
                        //kesitDTO.RezGenelList[i][j].Checked = true;
                        #region Özdirenç Değerinin Sağında Olan Sismik Değerlerinin Kontrolü
                        if (kesitDTO.SeismicGeneralList.Count >= kesitDTO.ResistivityGeneralList.Count)
                        {
                            CizimeSismikEkle(kesitDTO.ResistivityGeneralList[i][j].X, kesitDTO.SeismicGeneralList[i], kesitDTO.ResistivityGeneralList[i], dataset, (byte)Enums.DirectionValue.Right, j);
                        }
                        //    var sonOzdirencX = kesitDTO.RezGenelList[i][j].X;
                        //    var sagdaKalanSismikList = kesitDTO.SisGenelList[i].Where(s => s.X > sonOzdirencX).ToList();
                        //var ads = kesitDTO.SisGenelList[i];
                        //    foreach (var item in sagdaKalanSismikList)
                        //    {
                        //        coordinates = new List<double>();
                        //        coordinates.Add(item.X);
                        //        coordinates.Add((double)item.K);
                        //        dataset.data.Add(coordinates);
                        //    }

                        #endregion
                    }
                }
                if (dataset.data.Count > 0)
                    datasetList.Add(dataset);
            }
            //datasetList.RemoveAt(datasetList.Count - 1);//Sondaki fazla çizimin kaldırılması
            return datasetList;
        }

        private bool KapatmaKontrolu(List<SeriesDTO> datasetList, SeriesDTO dataset, List<List<ResistivityDTO>> rezGenelList, int indexI, int indexJ)
        {
            var ustDugum = rezGenelList[indexI - 1][indexJ];
            var ustSolDugum = rezGenelList[indexI - 1][indexJ - 1];
            foreach (var item in datasetList)
            {
                for (int i = 0; i < item.data.Count; i++)
                {
                    if (i + 1 < item.data.Count)
                    {
                        if (item.data[i].Count > 0 && item.data[i][0] == ustSolDugum.X && item.data[i][1] == ustSolDugum.K && item.data[i + 1][0] == ustDugum.X && item.data[i + 1][1] == ustDugum.K)
                        {
                            return true;
                        }
                    }

                }
            }

            return false;
        }

        private SeriesDTO KapatmaOlustur(List<SeriesDTO> datasetList, SeriesDTO dataset, List<List<ResistivityDTO>> rezGenelList, int i, int j)
        {
            if (rezGenelList.Count > i)
            {
                if (rezGenelList[i - 1].Count > j)
                {
                    List<double> coordinates;

                    double ortaNoktaX = 0, ortaNoktaK = 0;

                    ortaNoktaX = (rezGenelList[i - 1][j - 1].X + rezGenelList[i - 1][j].X) / 2;
                    ortaNoktaK = (double)(rezGenelList[i - 1][j - 1].K + rezGenelList[i - 1][j].K) / 2;

                    var oncekiNoktaX = rezGenelList[i - 1][j].X;
                    var oncekiNoktaK = rezGenelList[i - 1][j].K;

                    coordinates = new List<double>();
                    coordinates.Add(ortaNoktaX);
                    coordinates.Add((double)ortaNoktaK);
                    //datasetList.FirstOrDefault(d => d.name == oncekiDatasetName).data.Insert(index + 1, coordinates);
                    dataset.data.Add(coordinates);
                }
            }
            return dataset;
        }

        private SeriesDTO IlkDugumunSolundaFayKontrolu(SectionDTO kesitDTO, NodeDTO uygunIlkDugum, NodeDTO uygunIkinciDugum, List<SeriesDTO> datasetList)
        {

            if (uygunIlkDugum.IndexJ > 0 && uygunIlkDugum.IndexI > 0)
            {
                var fay = datasetList.Where(f => f.name == "Fay" && f.data[0][0] < uygunIlkDugum.Node.X && kesitDTO.ResistivityGeneralList[uygunIlkDugum.IndexI][uygunIlkDugum.IndexJ - 1].X < f.data[0][0]).FirstOrDefault();
                if (fay != null)
                {
                    return fay;
                }
            }

            return null;
        }


        private SeriesDTO IlkDugumunSagindaFayKontrolu(SectionDTO kesitDTO, NodeDTO uygunIlkDugum, NodeDTO uygunIkinciDugum, List<SeriesDTO> datasetList)
        {
            if (uygunIlkDugum.IndexJ > 0 && uygunIlkDugum.IndexI > 0)
            {
                var fay = datasetList.Where(f => f.name == "Fay" && f.data[0][0] > uygunIlkDugum.Node.X && uygunIkinciDugum.Node.X > f.data[0][0]).FirstOrDefault();
                if (fay != null)
                {
                    return fay;
                }
            }

            return null;
        }
        private bool FayKontrolu(GetRuleDTO kuralGetir, SectionDTO kesitDTO, NodeDTO uygunIlkDugum, NodeDTO uygunIkinciDugum, ParametersDTO parameters)
        {
            bool ilkOzdirencIleAlttakiUyumlumu = false, ikinciOzdirencIleAlttakiUyumlumu = false;
            bool ikiOzdirencKarsilastirma = true, VpUygunMu = false, VsUygunMu = false, altIkiOzdirencKarsilastirma = true, altVpUygunMu = false, altVsUygunMu = false;
            int i = uygunIlkDugum.IndexI;
            int j = uygunIlkDugum.IndexJ;
            //Sonuçlar False Dönmeli

            ikiOzdirencKarsilastirma = _fuzzyManager.FuzzyKuralOlusturVeSonucGetirFLLKarsilastirma(kuralGetir, (double)kesitDTO.ResistivityGeneralList[uygunIlkDugum.IndexI][uygunIlkDugum.IndexJ].R, (double)kesitDTO.ResistivityGeneralList[uygunIkinciDugum.IndexI][uygunIkinciDugum.IndexJ].R, (int)parameters.ResistivityRatio);
            VpUygunMu = SismikKontroluVp(kesitDTO, i, j, (int)parameters.SeismicRatio);
            VsUygunMu = SismikKontroluVs(kesitDTO, i, j, (int)parameters.SeismicRatio);

            if (uygunIlkDugum.IndexI + 1 + 1 < (double)kesitDTO.ResistivityGeneralList.Count && uygunIkinciDugum.IndexI + 1 + 1 < (double)kesitDTO.ResistivityGeneralList.Count)
            {
                //Sonuçlar False Dönmeli
                altIkiOzdirencKarsilastirma = _fuzzyManager.FuzzyKuralOlusturVeSonucGetirFLLKarsilastirma(kuralGetir, (double)kesitDTO.ResistivityGeneralList[uygunIlkDugum.IndexI + 1][uygunIlkDugum.IndexJ].R, (double)kesitDTO.ResistivityGeneralList[uygunIkinciDugum.IndexI + 1][uygunIkinciDugum.IndexJ].R, (int)parameters.ResistivityRatio);
                altVpUygunMu = SismikKontroluVp(kesitDTO, i + 1, j, (int)parameters.SeismicRatio);
                altVsUygunMu = SismikKontroluVs(kesitDTO, i + 1, j, (int)parameters.SeismicRatio);
            }

            if (uygunIkinciDugum.IndexI + 2 < (double)kesitDTO.ResistivityGeneralList.Count)
            {
                for (int k = i; k < (double)kesitDTO.ResistivityGeneralList[i].Count; k++)
                {
                    if ((double)kesitDTO.ResistivityGeneralList[i][k].TypeID == (byte)Enums.ExcelDataType.Real)
                    {
                        ilkOzdirencIleAlttakiUyumlumu = _fuzzyManager.FuzzyKuralOlusturVeSonucGetirFLLKarsilastirma(kuralGetir, (double)kesitDTO.ResistivityGeneralList[uygunIlkDugum.IndexI][uygunIlkDugum.IndexJ].R, (double)kesitDTO.ResistivityGeneralList[uygunIkinciDugum.IndexI + 2][uygunIkinciDugum.IndexJ].R, (int)parameters.ResistivityRatio);
                        break;
                    }
                }
            }
            if (uygunIlkDugum.IndexI + 1 < (double)kesitDTO.ResistivityGeneralList.Count && uygunIkinciDugum.IndexI + 3 < (double)kesitDTO.ResistivityGeneralList.Count)
            {
                for (int k = i + 1; k < (double)kesitDTO.ResistivityGeneralList[i + 1].Count; k++)
                {
                    if ((double)kesitDTO.ResistivityGeneralList[i + 1][k].TypeID == (byte)Enums.ExcelDataType.Real)
                    {
                        ikinciOzdirencIleAlttakiUyumlumu = _fuzzyManager.FuzzyKuralOlusturVeSonucGetirFLLKarsilastirma(kuralGetir, (double)kesitDTO.ResistivityGeneralList[uygunIlkDugum.IndexI + 1][uygunIlkDugum.IndexJ].R, (double)kesitDTO.ResistivityGeneralList[uygunIkinciDugum.IndexI + 3][uygunIkinciDugum.IndexJ].R, (int)parameters.ResistivityRatio);
                        break;
                    }
                }
            }

            if (!ikiOzdirencKarsilastirma &&
                VpUygunMu &&
                VsUygunMu &&
                !altIkiOzdirencKarsilastirma &&
                altVpUygunMu &&
                altVsUygunMu &&
                ilkOzdirencIleAlttakiUyumlumu &&
                ikinciOzdirencIleAlttakiUyumlumu)
                return true;

            return false;
        }

        private SeriesDTO FayOlustur(GetRuleDTO kuralGetir, SectionDTO kesitDTO, NodeDTO uygunIlkDugum, NodeDTO uygunIkinciDugum, ParametersDTO parameters)
        {

            //üst nokta belirle
            var FayBaslangicX = (kesitDTO.ResistivityGeneralList[uygunIlkDugum.IndexI - 2][uygunIlkDugum.IndexJ].X + kesitDTO.ResistivityGeneralList[uygunIkinciDugum.IndexI - 1][uygunIkinciDugum.IndexJ].X) / 2;
            var FayBaslangicY = (kesitDTO.ResistivityGeneralList[uygunIlkDugum.IndexI - 2][uygunIlkDugum.IndexJ].K + kesitDTO.ResistivityGeneralList[uygunIkinciDugum.IndexI - 1][uygunIkinciDugum.IndexJ].K) / 2;
            //alt nokta belirle

            var FayBitisX = (kesitDTO.ResistivityGeneralList[uygunIlkDugum.IndexI + 1][uygunIlkDugum.IndexJ].X + kesitDTO.ResistivityGeneralList[uygunIkinciDugum.IndexI + 3][uygunIkinciDugum.IndexJ].X) / 2;
            var FayBitisY = kesitDTO.ResistivityGeneralList[uygunIkinciDugum.IndexI + 3][uygunIkinciDugum.IndexJ].K;
            //Fay ciz
            SeriesDTO fayDataset = new SeriesDTO();
            fayDataset.name = "Fay";
            if ((bool)parameters.IsGraphsVisible)
                fayDataset.lineWidth = 2;
            fayDataset.color = "#000000";
            fayDataset.showInLegend = false;
            fayDataset.marker = new MarkerDTO { enabled = false };
            fayDataset.tooltip = new ToolTipDTO { useHTML = true };
            fayDataset.states = new StatesDTO { hover = new HoverDTO { lineWidthPlus = 3 } };
            //fayDataset.enableMouseTracking = false;
            fayDataset.draggableY = true;
            fayDataset.draggableX = true;

            List<double> coordinates = new List<double>();
            coordinates.Add(FayBaslangicX);
            coordinates.Add((double)FayBaslangicY);
            fayDataset.data.Add(coordinates);

            coordinates = new List<double>();
            coordinates.Add(FayBitisX);
            coordinates.Add((double)FayBitisY);
            fayDataset.data.Add(coordinates);

            return fayDataset;
        }

        private string RenkUret(int i, int count)
        {

            var color = Color.Black;
            if (i == 0) //ilk Çizgi Mavi
            {
                color = Color.FromArgb(0, 0, 255);
                return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
            }
            else if (count - 1 == i) //Son Çizgi Kırmızı
            {
                color = Color.FromArgb(255, 0, 0);
                return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
            }
            else
            {
                List<RGBDTO> RGBList = ColorsFromBlueToRed();
                double percent = (double)i / (double)count * 100;
                int index = (int)Math.Round(percent * RGBList.Count / 100);
                RGBDTO RGBItem = RGBList[index];

                return "#" + RGBItem.R.ToString("X2") + RGBItem.G.ToString("X2") + RGBItem.B.ToString("X2");
            }
        }

        private List<RGBDTO> ColorsFromBlueToRed()
        {
            List<RGBDTO> RGBList = new List<RGBDTO>();

            RGBList.Add(new RGBDTO
            {
                R = 0,
                G = 0,
                B = 255
            });


            for (int i = 1; i < 256; i++) //Maviden Açık Maviye
            {
                RGBList.Add(new RGBDTO
                {
                    R = 0,
                    G = i,
                    B = 255
                });
            }

            for (int i = 254; i >= 0; i--) //Açık Maviden Yeşile
            {
                RGBList.Add(new RGBDTO
                {
                    R = 0,
                    G = 255,
                    B = i
                });
            }

            for (int i = 1; i < 256; i++) //Yeşilden Sarıya
            {
                RGBList.Add(new RGBDTO
                {
                    R = i,
                    G = 255,
                    B = 0
                });
            }

            for (int i = 254; i >= 0; i--) //Sarıdan Kırmızıya
            {
                RGBList.Add(new RGBDTO
                {
                    R = 255,
                    G = i,
                    B = 0
                });
            }

            return RGBList;
        }

        public ResultDTO KumeListesiGetir()
        {
            ResultDTO sonuc = new ResultDTO();
            try
            {
                var kuralList = _ruleService.Queryable().Where(k => k.AktifMi == true).Select(k => new RuleEntityDTO
                {
                    RuleId = k.KuralID,
                    RuleName = k.KuralAdi,
                    AddDate = k.EklenmeTarihi,
                    IsActive = k.AktifMi
                }).ToList();
                sonuc.Object = kuralList;
                sonuc.Result = true;
                sonuc.Message = "Başarılı.";
                return sonuc;
            }
            catch (Exception ex)
            {

                sonuc.Object = null;
                sonuc.Result = false;
                sonuc.Message = "Başarısız.";
                sonuc.Exception = ex;
                return sonuc;
            }
        }

        public ResultDTO KuralGetir(long kuralID)
        {
            ResultDTO sonuc = new ResultDTO();
            try
            {
                var kuralList = _ruleListTextService.Queryable().Where(k => k.KuralID == kuralID).Select(k => new RuleTextEntityDTO
                {
                    RuleId = k.KuralID,
                    RuleText = k.KuralText
                }).ToList();
                sonuc.Object = kuralList;
                sonuc.Result = true;
                sonuc.Message = "Başarılı.";
                return sonuc;
            }
            catch (Exception ex)
            {

                sonuc.Object = null;
                sonuc.Result = false;
                sonuc.Message = "Başarısız.";
                sonuc.Exception = ex;
                return sonuc;
            }
        }

        public ResultDTO KuralTextVeOzdirencGetir(long kuralID)
        {
            ResultDTO sonuc = new ResultDTO();
            try
            {
                var kuralList = _ruleListTextService.Queryable().Where(k => k.KuralID == kuralID).Select(k => new RuleTextEntityDTO
                {
                    RuleId = k.KuralID,
                    RuleText = k.KuralText
                }).ToList();
                var ozdirencList = _variableItemService.Queryable().Where(d => d.Degisken.KuralID == kuralID && d.Degisken.DegiskenTipID == (byte)Enums.VariableType.Input).Select(d => new VariableDTO
                {
                    Name = d.DegiskenItemAdi,
                    MinValue = d.MinDeger,
                    MaxValue = d.MaxDeger,
                }).ToList();
                sonuc.Object = new RuleTextAndResistivityDTO { ruleTextList = kuralList, resistivityList = ozdirencList };
                sonuc.Result = true;
                sonuc.Message = "Başarılı.";
                return sonuc;
            }
            catch (Exception ex)
            {

                sonuc.Object = null;
                sonuc.Result = false;
                sonuc.Message = "Başarısız.";
                sonuc.Exception = ex;
                return sonuc;
            }
        }

        private bool SismikKontroluVp(SectionDTO kesitDTO, int i, int j, int oran)
        {
            if (i < kesitDTO.SeismicGeneralList.Count && j < kesitDTO.SeismicGeneralList[i].Count)
            {
                if (kesitDTO.SeismicGeneralList[i][j].Vp != null && kesitDTO.SeismicGeneralList[i][j].Vp != 0 && kesitDTO.SeismicGeneralList[i][j].Vs != null && kesitDTO.SeismicGeneralList[i][j].Vs != 0)
                {
                    if ((i + 1) < kesitDTO.SeismicGeneralList.Count)
                    {
                        if ((kesitDTO.SeismicGeneralList[i][j].X > kesitDTO.ResistivityGeneralList[i][j].X && kesitDTO.SeismicGeneralList[i][j].X < kesitDTO.ResistivityGeneralList[i + 1][j].X) && (kesitDTO.SeismicGeneralList[i + 1][j].X > kesitDTO.ResistivityGeneralList[i][j].X && kesitDTO.SeismicGeneralList[i + 1][j].X < kesitDTO.ResistivityGeneralList[i + 1][j].X)) //iki özdirenç arasında birden fazla sismik ölçüm olma durumu
                        {
                            if (kesitDTO.SeismicGeneralList[i][j].Vp > kesitDTO.SeismicGeneralList[i + 1][j].Vp)//soldaki Vp daha büyükse
                            {
                                if (kesitDTO.SeismicGeneralList[i][j].Vp * (oran / 100) > kesitDTO.SeismicGeneralList[i + 1][j].Vp) //öncekinin oran ile çarpımı bir sonrakinden büyük olmalı
                                {
                                    return false;
                                }
                            }
                            else //sağdaki daha büyükse
                            {
                                if (kesitDTO.SeismicGeneralList[i + 1][j].Vp * (oran / 100) > kesitDTO.SeismicGeneralList[i][j].Vp)
                                {
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            if (j + 1 < kesitDTO.SeismicGeneralList[i].Count)
                            {
                                if (kesitDTO.SeismicGeneralList[i][j].Vp * (oran / 100) > kesitDTO.SeismicGeneralList[i][j + 1].Vp) //öncekinin oran ile çarpımı bir sonrakinden büyük olmalı
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }

        private bool SismikKontroluVs(SectionDTO kesitDTO, int i, int j, int oran)
        {
            if (i < kesitDTO.SeismicGeneralList.Count && j < kesitDTO.SeismicGeneralList[i].Count)
            {
                if (kesitDTO.SeismicGeneralList[i][j].Vs != null && kesitDTO.SeismicGeneralList[i][j].Vs != 0 && kesitDTO.SeismicGeneralList[i][j].Vs != null && kesitDTO.SeismicGeneralList[i][j].Vs != 0)
                {
                    if ((i + 1) < kesitDTO.SeismicGeneralList.Count)
                    {
                        if ((kesitDTO.SeismicGeneralList[i][j].X > kesitDTO.ResistivityGeneralList[i][j].X && kesitDTO.SeismicGeneralList[i][j].X < kesitDTO.ResistivityGeneralList[i + 1][j].X) && (kesitDTO.SeismicGeneralList[i + 1][j].X > kesitDTO.ResistivityGeneralList[i][j].X && kesitDTO.SeismicGeneralList[i + 1][j].X < kesitDTO.ResistivityGeneralList[i + 1][j].X)) //iki özdirenç arasında birden fazla sismik ölçüm olma durumu
                        {
                            if (kesitDTO.SeismicGeneralList[i][j].Vs > kesitDTO.SeismicGeneralList[i + 1][j].Vs)//soldaki Vs daha büyükse
                            {
                                if (kesitDTO.SeismicGeneralList[i][j].Vs * (oran / 100) > kesitDTO.SeismicGeneralList[i + 1][j].Vs) //öncekinin %60 bir sonrakinden büyük olmalı
                                {
                                    return false;
                                }
                            }
                            else //sağdaki daha büyükse
                            {
                                if (kesitDTO.SeismicGeneralList[i + 1][j].Vs * (oran / 100) > kesitDTO.SeismicGeneralList[i][j].Vs)
                                {
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            if (j + 1 < kesitDTO.SeismicGeneralList[i].Count)
                            {
                                if (kesitDTO.SeismicGeneralList[i][j].Vs * (oran / 100) > kesitDTO.SeismicGeneralList[i][j + 1].Vs) //öncekinin oran ile çarpımı bir sonrakinden büyük olmalı
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }

        private SeriesDTO CukurOlustur(SeriesDTO dataset, List<List<ResistivityDTO>> rezGenelList, int i, int j)
        {
            SeriesDTO cukurDataset = new SeriesDTO();
            cukurDataset.name = "Çukur";
            cukurDataset.lineWidth = 2;
            cukurDataset.color = dataset.color;
            cukurDataset.showInLegend = false;
            cukurDataset.marker = new MarkerDTO { enabled = false };
            cukurDataset.tooltip = new ToolTipDTO { useHTML = true };
            cukurDataset.states = new StatesDTO { hover = new HoverDTO { lineWidthPlus = 3 } };
            cukurDataset.enableMouseTracking = false;

            var cukurBaslangicX = rezGenelList[i][j].X - (Math.Abs(rezGenelList[i][j].X - rezGenelList[i][j - 1].X) / 5);
            var cukurBaslangicK = rezGenelList[i][j].K - (Math.Abs((double)rezGenelList[i][j].K - (double)rezGenelList[i][j - 1].K) / 5);

            var cukurBitisX = rezGenelList[i][j].X + (Math.Abs(rezGenelList[i][j].X - rezGenelList[i][j + 1].X) / 5);
            var cukurBitisK = rezGenelList[i][j].K + (Math.Abs((double)rezGenelList[i][j].K - (double)rezGenelList[i][j + 1].K) / 5);

            List<double> coordinates = new List<double>();
            coordinates.Add(rezGenelList[i][j - 1].X);
            coordinates.Add((double)rezGenelList[i][j - 1].K);
            dataset.data.Add(coordinates);

            coordinates = new List<double>();
            coordinates.Add(cukurBaslangicX);
            coordinates.Add((double)cukurBaslangicK);
            dataset.data.Add(coordinates);

            coordinates = new List<double>();
            coordinates.Add(rezGenelList[i][j].X);
            coordinates.Add((double)rezGenelList[i][j].K);
            dataset.data.Add(coordinates);

            coordinates = new List<double>();
            coordinates.Add(cukurBitisX);
            coordinates.Add((double)cukurBitisK);
            dataset.data.Add(coordinates);

            coordinates = new List<double>();
            coordinates.Add(rezGenelList[i][j + 1].X);
            coordinates.Add((double)rezGenelList[i][j + 1].K);
            dataset.data.Add(coordinates);



            coordinates = new List<double>();
            coordinates.Add(cukurBaslangicX);
            coordinates.Add((double)cukurBaslangicK);
            cukurDataset.data.Add(coordinates);

            coordinates = new List<double>();
            coordinates.Add(rezGenelList[i][j].X + 1);
            coordinates.Add((double)rezGenelList[i][j].K);
            cukurDataset.data.Add(coordinates);

            coordinates = new List<double>();
            coordinates.Add(cukurBitisX);
            coordinates.Add((double)cukurBitisK);
            cukurDataset.data.Add(coordinates);

            return cukurDataset;
        }

        /// <summary>
        /// Özdirenç Değerinin Sağında Veya Solunda Bulunan Sismik Değerlerini Çizime Bağlar
        /// </summary>
        private void CizimeSismikEkle(double OzdirencX, List<SeismicDTO> sismikList, List<ResistivityDTO> rezistiviteList, SeriesDTO dataset, byte Yon, int j)
        {

            var SismikList = sismikList.Where(s => Yon == (byte)Enums.DirectionValue.Left ? s.X < OzdirencX : s.X > OzdirencX).ToList();
            for (int i = 0; i < SismikList.Count; i++)
            {
                GraphDetailedDTO cizimDetailed = new GraphDetailedDTO();

                List<double> coordinates = new List<double>();
                coordinates.Add(SismikList[i].X);
                coordinates.Add((double)SismikList[i].K);
                dataset.data.Add(coordinates);


                if (Yon == (byte)Enums.DirectionValue.Left)//Çizimin solundaki sismik değerleri kontrol ediliyorsa
                {
                    if (SismikList.Count > 1)
                    {
                        if (i < SismikList.Count - 1)
                        {
                            cizimDetailed = new GraphDetailedDTO { FirstNode = SismikList[i].Name, SecondNode = SismikList[i + 1].Name, Normal = true, Connection = "Normal" };
                            cizimDetailedList.Add(cizimDetailed);
                        }
                        else
                        {
                            cizimDetailed = new GraphDetailedDTO { FirstNode = SismikList[i].Name, SecondNode = rezistiviteList[j].Name, Normal = true, Connection = "Normal" };
                            cizimDetailedList.Add(cizimDetailed);
                        }
                    }
                    else
                    {
                        cizimDetailed = new GraphDetailedDTO { FirstNode = SismikList[i].Name, SecondNode = rezistiviteList[j].Name, Normal = true, Connection = "Normal" };
                        cizimDetailedList.Add(cizimDetailed);
                    }
                }
                else//Çizimin sağındaki sismik değerleri kontrol ediliyorsa
                {
                    if (SismikList.Count > 1)
                    {
                        if (i < SismikList.Count - 1)
                        {
                            cizimDetailed = new GraphDetailedDTO { FirstNode = SismikList[i].Name, SecondNode = SismikList[i+1].Name, Normal = true, Connection = "Normal" };
                            cizimDetailedList.Add(cizimDetailed);
                        }
                        else
                        {
                            cizimDetailed = new GraphDetailedDTO { FirstNode = rezistiviteList[j].Name, SecondNode = SismikList[i].Name, Normal = true, Connection = "Normal" };
                            cizimDetailedList.Add(cizimDetailed);
                        }
                    }
                    else
                    {
                        cizimDetailed = new GraphDetailedDTO { FirstNode = rezistiviteList[j].Name, SecondNode = SismikList[i].Name, Normal = true, Connection = "Normal" };
                        cizimDetailedList.Add(cizimDetailed);
                    }
                }



                cizimCount.Normal++;
            }
        }


        private NodeDTO UygunIlkDugumKontrolu(List<List<ResistivityDTO>> rezGenelList, int indexI, int indexJ)
        {
            NodeDTO dugum = new NodeDTO { Node = rezGenelList[indexI][indexJ], IndexI = indexI, IndexJ = indexJ };

            for (int i = indexI - 1; i >= 0; i--)
            {
                if (!rezGenelList[i][indexJ].Checked && rezGenelList[i][indexJ].TypeID == (byte)Enums.ExcelDataType.Real)
                {
                    if (!rezGenelList[i - 1][indexJ].Checked)
                    {
                        return dugum;
                    }
                    dugum.IndexI = i;
                    dugum.IndexJ = indexJ;
                    dugum.Node = rezGenelList[i][indexJ];
                    break;
                }
            }


            return dugum;
        }

        private NodeDTO UygunIkinciDugumKontrolu(GetRuleDTO kuralGetir, SectionDTO kesitDTO, List<List<ResistivityDTO>> rezGenelList, int indexI, int indexJ, ParametersDTO parameters)
        {
            NodeDTO dugum = new NodeDTO { Node = rezGenelList[indexI][indexJ], IndexI = indexI, IndexJ = indexJ };

            //bool ikiOzdirencKarsilastirma = _fuzzyManager.FuzzyKuralOlusturVeSonucGetirFLLKarsilastirma(kuralGetir, (double)kesitDTO.RezGenelList[indexI][indexJ - 1].R, (double)kesitDTO.RezGenelList[indexI][indexJ].R, (int)parameters.OzdirencOran);
            //if (ikiOzdirencKarsilastirma)
            //{
            //    return dugum;
            //}

            for (int i = indexI - 1; i >= 0; i--)
            {
                if (!rezGenelList[i][indexJ].Checked && rezGenelList[i][indexJ].TypeID == (byte)Enums.ExcelDataType.Real)
                {

                    dugum.IndexI = i;
                    dugum.IndexJ = indexJ;
                    dugum.Node = rezGenelList[i][indexJ];
                }
            }


            return dugum;
        }

        #region Eski Ayrı Ayrı Kontrol Kodları
        private List<SeriesDTO> GraphDataOlustur(List<List<ResistivityDTO>> rezGenelList)
        {
            List<SeriesDTO> datasetList = new List<SeriesDTO>();
            SeriesDTO dataset;
            var name = "Set-";
            int count = 0;
            var random = new Random();
            foreach (var rezList in rezGenelList)
            {
                count++;
                dataset = new SeriesDTO();
                dataset.name = name + count.ToString();
                dataset.lineWidth = 2;
                dataset.color = String.Format("#{0:X6}", random.Next(0x1000000));
                dataset.showInLegend = false;
                dataset.marker = new MarkerDTO { symbol = "circle", radius = 2, enabled = true };
                dataset.tooltip = new ToolTipDTO { useHTML = true };
                dataset.states = new StatesDTO { hover = new HoverDTO { lineWidthPlus = 3 } };
                dataset.enableMouseTracking = false;
                for (int i = 0; i < rezList.Count; i++)
                {
                    List<double> coordinates = new List<double>();
                    //if (i != rezList.Count - 1) //Son sıra kontrolü
                    //{
                    //    if (rezList[i].R * 0.3 >= rezList[i + 1].R || rezList[i + 1].R * 0.3 >= rezList[i].R) //Bir sonraki özdirenç değerinin kontrolü 
                    //    {
                    //        if (i > 0) //çukur kontrolü yapabilmek için bir önceki özdirençe değerine bakmak gerek. Burada hangi sırada olduğunun kontrolü 
                    //        {
                    //            if(rezList[i - 1].R * 0.3 >= rezList[i].R || rezList[i].R * 0.3 >= rezList[i - 1].R)//Çukur oluşturulacak
                    //            {

                    //            }
                    //            else//Fay oluşturulacak
                    //            {

                    //            }
                    //        } else
                    //        {
                    //            coordinates.Add(rezList[i].X);
                    //            coordinates.Add((double)rezList[i].K);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        coordinates.Add(rezList[i].X);
                    //        coordinates.Add((double)rezList[i].K);
                    //    }
                    //}
                    coordinates.Add(rezList[i].X);
                    coordinates.Add((double)rezList[i].K);
                    dataset.data.Add(coordinates);
                }
                datasetList.Add(dataset);
            }
            return datasetList;
        }
        private List<SeriesDTO> GraphDataOlustur(List<List<SeismicDTO>> sisGenelList)
        {
            List<SeriesDTO> datasetList = new List<SeriesDTO>();
            SeriesDTO dataset;
            var name = "Set-";
            int count = 0;
            var random = new Random();
            foreach (var sisList in sisGenelList)
            {
                count++;
                dataset = new SeriesDTO();
                dataset.name = name + count.ToString();
                dataset.lineWidth = 2;
                dataset.color = String.Format("#{0:X6}", random.Next(0x1000000));
                dataset.showInLegend = false;
                dataset.marker = new MarkerDTO { symbol = "triangle-down", radius = 2, enabled = true };
                dataset.tooltip = new ToolTipDTO { useHTML = true };
                dataset.states = new StatesDTO { hover = new HoverDTO { lineWidthPlus = 3 } };
                dataset.enableMouseTracking = false;
                foreach (var rezItem in sisList)
                {
                    List<double> coordinates = new List<double>();
                    coordinates.Add(rezItem.X);
                    coordinates.Add((double)rezItem.K);
                    dataset.data.Add(coordinates);
                }
                datasetList.Add(dataset);
            }
            return datasetList;
        }
        private List<SeriesDTO> GraphDataOlustur(List<List<DrillDTO>> sonGenelList)
        {
            List<SeriesDTO> datasetList = new List<SeriesDTO>();
            SeriesDTO dataset;
            var name = "Set-";
            int count = 0;
            var random = new Random();
            foreach (var rezList in sonGenelList)
            {
                count++;
                dataset = new SeriesDTO();
                dataset.name = name + count.ToString();
                dataset.lineWidth = 0;
                dataset.color = String.Format("#{0:X6}", random.Next(0x1000000));
                dataset.showInLegend = false;
                dataset.marker = new MarkerDTO { symbol = "triangle", radius = 2, enabled = true };
                dataset.tooltip = new ToolTipDTO { useHTML = true };
                dataset.states = new StatesDTO { hover = new HoverDTO { lineWidthPlus = 0 } };
                dataset.enableMouseTracking = false;
                foreach (var rezItem in rezList)
                {
                    List<double> coordinates = new List<double>();
                    coordinates.Add(rezItem.X);
                    coordinates.Add((double)rezItem.K);
                    dataset.data.Add(coordinates);
                }
                datasetList.Add(dataset);
            }
            return datasetList;
        }
        #endregion
    }

    public interface IGraphManager : IBaseManager
    {
        ResultDTO ExcelKontrolEt(ExcelModelDTO excel, string path);
        ResultDTO GraphOlustur(GraphDTO graph, string path);
        List<SeriesDTO> GraphDataOlustur(long kuralID, SectionDTO kesitDTO, ParametersDTO parameters);
        ResultDTO KumeListesiGetir();
        ResultDTO KuralGetir(long kuralID);
        ResultDTO KuralTextVeOzdirencGetir(long kuralID);
    }
}
