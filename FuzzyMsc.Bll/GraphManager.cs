using FuzzyMsc.Bll.Interface;
using FuzzyMsc.Core.Enums;
using FuzzyMsc.Dto;
using FuzzyMsc.Dto.CizimDTOS;
using FuzzyMsc.Dto.FuzzyDTOS;
using FuzzyMsc.Dto.HighchartsDTOS;
using FuzzyMsc.Pattern.UnitOfWork;
using FuzzyMsc.Service;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections;
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
        IOrtakManager _ortakManager;
        IKullaniciService _kullaniciService;
        IKuralService _kuralService;
        IKuralListService _kuralListService;
        IKuralListItemService _kuralListItemService;
        IKuralListTextService _kuralListTextService;
        IDegiskenService _degiskenService;
        IDegiskenItemService _degiskenItemService;
        IFuzzyManager _fuzzyManager;

        private List<List<RezistiviteDTO>> rezGenelList;
        private List<List<SismikDTO>> sisGenelList;
        private List<List<SondajDTO>> sonGenelList;
        private int id;
        Microsoft.Office.Interop.Excel.Application xl;
        Microsoft.Office.Interop.Excel.Workbook xlWorkbook;

        [DllImport("user32.dll")]
        static extern int GetWindowThreadProcessId(int hWnd, out int lpdwProcessId);

        public GraphManager(
            IUnitOfWorkAsync unitOfWork,
            IKullaniciService kullaniciService,
            IOrtakManager ortakManager,
            IKuralService kuralService,
            IKuralListService kuralListService,
            IKuralListItemService kuralListItemService,
            IKuralListTextService kuralListTextService,
            IDegiskenService degiskenService,
            IDegiskenItemService degiskenItemService,
            IFuzzyManager fuzzyManager)
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
            _fuzzyManager = fuzzyManager;
        }

        public SonucDTO ExcelKontrolEt(ExcelModelDTO excel, string path)
        {
            File.WriteAllBytes(path, Convert.FromBase64String(excel.data));

            return null;


        }
        public SonucDTO GraphOlustur(GraphDTO graph, string path)
        {
            try
            {
                SonucDTO sonuc = new SonucDTO();
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
                KesitDTO kesitDTO = new KesitDTO { RezGenelList = rezGenelList, SisGenelList = sisGenelList, SonGenelList = sonGenelList };
                highcharts.series.AddRange(GraphDataOlustur(graph.kuralID, kesitDTO));

                double minX = MinHesapla(highcharts);
                highcharts.xAxis = new AxisDTO { min = 0, minTickInterval = 0.5, offset = 20, title = new AxisTitleDTO { text = "Genişlik" }, labels = new AxisLabelsDTO { format = "{value} m" } };
                highcharts.yAxis = new AxisDTO { min = (int)minX - 15, minTickInterval = 0.5, offset = 20, title = new AxisTitleDTO { text = "Yükseklik" }, labels = new AxisLabelsDTO { format = "{value} m" } };
                sonuc.Nesne = highcharts;
                sonuc.Sonuc = true;
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

        private double MinHesapla(HighchartsDTO highcharts)
        {
            double min = Double.MaxValue;
            foreach (var item in highcharts.series)
            {
                var asd = item.data.FirstOrDefault();
                if (asd != null)
                {
                    if (asd.Count > 0)
                    {
                        double a = asd[1];
                        if (a < min)
                            min = a;
                    }
                }

            }
            return min;
        }


        private void RezistiviteOlustur(HighchartsDTO highcharts, Workbook xlWorkbook)
        {
            rezGenelList = new List<List<RezistiviteDTO>>();
            List<RezistiviteDTO> rezList = new List<RezistiviteDTO>();
            RezistiviteDTO rezItem = new RezistiviteDTO();
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

            for (int i = 1; i < rowCount; i++)
            {
                rezItem = new RezistiviteDTO();
                rezItem.ID = i;
                rezItem.Adi = (string)(xlWorksheetRezistivite.Cells[i + 1, 1]).Value.ToString();
                rezItem.X = (double)(xlWorksheetRezistivite.Cells[i + 1, 2]).Value;
                rezItem.K = (double)(xlWorksheetRezistivite.Cells[i + 1, 4]).Value;
                rezList.Add(rezItem);
            }
            rezGenelList.Add(rezList);

            int count = 1;
            for (int j = 5; j <= colCount; j = j + 2)
            {
                rezList = new List<RezistiviteDTO>();
                for (int i = 1; i <= rowCount; i++)
                {

                    rezItem = new RezistiviteDTO();
                    if ((xlWorksheetRezistivite.Cells[i + 1, j]).Value == null && (xlWorksheetRezistivite.Cells[i + 1, j + 1]).Value == null)
                    {
                        continue;
                    }
                    if ((xlWorksheetRezistivite.Cells[i + 1, j]).Value == null && (xlWorksheetRezistivite.Cells[i + 1, j + 1]).Value != null)
                    {
                        rezItem.ID = i;
                        rezItem.Adi = (string)(xlWorksheetRezistivite.Cells[i + 1, 1]).Value.ToString() + count.ToString();
                        rezItem.X = (double)(xlWorksheetRezistivite.Cells[i + 1, 2]).Value;
                        rezItem.K = ((double)(xlWorksheetRezistivite.Cells[i + 1, 4]).Value - (double)(xlWorksheetRezistivite.Cells[i + 1, j - 2]).Value) * 0.99;
                        rezItem.R = (xlWorksheetRezistivite.Cells[i + 1, j + 1]).Value == null ? 0 : (double)(xlWorksheetRezistivite.Cells[i + 1, j + 1]).Value;
                        rezList.Add(rezItem);
                        count++;
                        continue;
                    }
                    rezItem.ID = i;
                    rezItem.Adi = (string)(xlWorksheetRezistivite.Cells[i + 1, 1]).Value.ToString() + count.ToString();
                    rezItem.X = (double)(xlWorksheetRezistivite.Cells[i + 1, 2]).Value;
                    rezItem.K = (xlWorksheetRezistivite.Cells[i + 1, j]).Value == null ? 0 : (double)(xlWorksheetRezistivite.Cells[i + 1, 4]).Value - (double)(xlWorksheetRezistivite.Cells[i + 1, j]).Value;
                    rezItem.R = (xlWorksheetRezistivite.Cells[i + 1, j + 1]).Value == null ? 0 : (double)(xlWorksheetRezistivite.Cells[i + 1, j + 1]).Value;
                    rezList.Add(rezItem);
                    count++;
                }
                rezGenelList.Add(rezList);
                count = 1;
            }

            highcharts = ChartOlustur(highcharts, rezGenelList);
        }
        private void SismikOlustur(HighchartsDTO highcharts, Workbook xlWorkbook)
        {
            sisGenelList = new List<List<SismikDTO>>();
            List<SismikDTO> sisList = new List<SismikDTO>();
            SismikDTO sisItem = new SismikDTO();
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

            for (int i = 1; i < rowCount; i++)
            {
                sisItem = new SismikDTO();
                sisItem.ID = i;
                sisItem.Adi = (string)(xlWorksheetSismik.Cells[i + 1, 1]).Value.ToString();
                sisItem.X = (double)(xlWorksheetSismik.Cells[i + 1, 2]).Value;
                sisItem.K = (double)(xlWorksheetSismik.Cells[i + 1, 4]).Value;
                sisList.Add(sisItem);
            }
            sisGenelList.Add(sisList);

            int count = 1;
            for (int j = 5; j <= colCount; j = j + 3)
            {
                sisList = new List<SismikDTO>();
                for (int i = 1; i <= rowCount; i++)
                {

                    sisItem = new SismikDTO();
                    if ((xlWorksheetSismik.Cells[i + 1, j]).Value == null && (xlWorksheetSismik.Cells[i + 1, j + 1]).Value == null && (xlWorksheetSismik.Cells[i + 1, j + 2]).Value == null)
                    {
                        continue;
                    }
                    if ((xlWorksheetSismik.Cells[i + 1, j]).Value == null && (xlWorksheetSismik.Cells[i + 1, j + 1]).Value != null && (xlWorksheetSismik.Cells[i + 1, j + 2]).Value != null)
                    {
                        sisItem.ID = i;
                        sisItem.Adi = (string)(xlWorksheetSismik.Cells[i + 1, 1]).Value.ToString() + count.ToString();
                        sisItem.X = (double)(xlWorksheetSismik.Cells[i + 1, 2]).Value;
                        sisItem.K = ((double)(xlWorksheetSismik.Cells[i + 1, 4]).Value - (double)(xlWorksheetSismik.Cells[i + 1, j - 3]).Value) * 0.99;
                        sisItem.Vp = (xlWorksheetSismik.Cells[i + 1, j + 1]).Value == null ? 0 : (double)(xlWorksheetSismik.Cells[i + 1, j + 1]).Value;
                        sisItem.Vs = (xlWorksheetSismik.Cells[i + 1, j + 2]).Value == null ? 0 : (double)(xlWorksheetSismik.Cells[i + 1, j + 2]).Value;
                        sisList.Add(sisItem);
                        count++;
                        continue;
                    }
                    sisItem.ID = i;
                    sisItem.Adi = (string)(xlWorksheetSismik.Cells[i + 1, 1]).Value.ToString() + count.ToString();
                    sisItem.X = (double)(xlWorksheetSismik.Cells[i + 1, 2]).Value;
                    sisItem.K = (xlWorksheetSismik.Cells[i + 1, j]).Value == null ? 0 : (double)(xlWorksheetSismik.Cells[i + 1, 4]).Value - (double)(xlWorksheetSismik.Cells[i + 1, j]).Value;
                    sisItem.Vp = (xlWorksheetSismik.Cells[i + 1, j + 1]).Value == null ? 0 : (double)(xlWorksheetSismik.Cells[i + 1, j + 1]).Value;
                    sisItem.Vs = (xlWorksheetSismik.Cells[i + 1, j + 2]).Value == null ? 0 : (double)(xlWorksheetSismik.Cells[i + 1, j + 2]).Value;
                    sisList.Add(sisItem);
                    count++;
                }
                sisGenelList.Add(sisList);
                count = 1;
            }

            highcharts = ChartOlustur(highcharts, sisGenelList);
        }
        private void SondajOlustur(HighchartsDTO highcharts, Workbook xlWorkbook)
        {
            sonGenelList = new List<List<SondajDTO>>();
            List<SondajDTO> sonList = new List<SondajDTO>();
            SondajDTO sonItem = new SondajDTO();
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
                sonItem = new SondajDTO();
                sonItem.ID = i;
                sonItem.Adi = (string)(xlWorkSheetSondaj.Cells[i + 1, 1]).Value.ToString();
                sonItem.X = (double)(xlWorkSheetSondaj.Cells[i + 1, 2]).Value;
                sonItem.K = (double)(xlWorkSheetSondaj.Cells[i + 1, 4]).Value;
                sonList.Add(sonItem);
            }
            sonGenelList.Add(sonList);

            int count = 1;
            for (int j = 5; j <= colCount; j = j + 2)
            {
                sonList = new List<SondajDTO>();
                for (int i = 1; i <= rowCount; i++)
                {

                    sonItem = new SondajDTO();
                    if ((xlWorkSheetSondaj.Cells[i + 1, j]).Value == null && (xlWorkSheetSondaj.Cells[i + 1, j + 1]).Value == null)
                    {
                        continue;
                    }
                    if ((xlWorkSheetSondaj.Cells[i + 1, j]).Value == null && (xlWorkSheetSondaj.Cells[i + 1, j + 1]).Value != null)
                    {
                        sonItem.ID = i;
                        sonItem.Adi = (string)(xlWorkSheetSondaj.Cells[i + 1, 1]).Value.ToString() + count.ToString();
                        sonItem.X = (double)(xlWorkSheetSondaj.Cells[i + 1, 2]).Value;
                        sonItem.K = ((double)(xlWorkSheetSondaj.Cells[i + 1, 4]).Value - (double)(xlWorkSheetSondaj.Cells[i + 1, j - 2]).Value) * 0.99;
                        sonItem.T = (xlWorkSheetSondaj.Cells[i + 1, j + 1]).Value == null ? "" : (xlWorkSheetSondaj.Cells[i + 1, j + 1]).Value;
                        sonList.Add(sonItem);
                        count++;
                        continue;
                    }
                    sonItem.ID = i;
                    sonItem.Adi = (string)(xlWorkSheetSondaj.Cells[i + 1, 1]).Value.ToString() + count.ToString();
                    sonItem.X = (double)(xlWorkSheetSondaj.Cells[i + 1, 2]).Value;
                    sonItem.K = (xlWorkSheetSondaj.Cells[i + 1, j]).Value == null ? 0 : (double)(xlWorkSheetSondaj.Cells[i + 1, 4]).Value - (double)(xlWorkSheetSondaj.Cells[i + 1, j]).Value;
                    sonItem.T = (xlWorkSheetSondaj.Cells[i + 1, j + 1]).Value == null ? "" : (xlWorkSheetSondaj.Cells[i + 1, j + 1]).Value;
                    sonList.Add(sonItem);
                    count++;
                }
                sonGenelList.Add(sonList);
                count = 1;
            }

            highcharts = ChartOlustur(highcharts, sonGenelList);
        }

        private HighchartsDTO ChartOlustur(HighchartsDTO highcharts, List<List<RezistiviteDTO>> rezGenelList)
        {
            //highcharts.series.AddRange(GraphDataOlustur(rezGenelList));
            highcharts.annotations.AddRange(GraphAnnotationsOlustur(rezGenelList));

            return highcharts;
        }

        private HighchartsDTO ChartOlustur(HighchartsDTO highcharts, List<List<SismikDTO>> sisGenelList)
        {
            //highcharts.series.AddRange(GraphDataOlustur(sisGenelList));
            highcharts.annotations.AddRange(GraphAnnotationsOlustur(sisGenelList));

            return highcharts;
        }
        private HighchartsDTO ChartOlustur(HighchartsDTO highcharts, List<List<SondajDTO>> sonGenelList)
        {
            //highcharts.series.AddRange(GraphDataOlustur(sonGenelList));
            highcharts.annotations.AddRange(GraphAnnotationsOlustur(sonGenelList));

            return highcharts;
        }

        private List<AnnotationsDTO> GraphAnnotationsOlustur(List<List<RezistiviteDTO>> rezGenelList)
        {
            List<AnnotationsDTO> annotationsList = new List<AnnotationsDTO>();
            AnnotationsDTO annotations;
            AnnotationLabelsDTO label;

            for (int i = 0; i < rezGenelList.Count; i++)
            {
                annotations = new AnnotationsDTO();
                annotations.visible = true;
                //annotations.labelOptions = new AnnotationLabelOptionsDTO { shape = "connector", align = "right", justify = false, crop = true, style = new StyleDTO { fontSize = "0.8em", textOutline = "1px white" } };
                foreach (var rezItem in rezGenelList[i])
                {
                    if (i == 0)
                        label = new AnnotationLabelsDTO { x = -20, y = -20, point = new PointDTO { xAxis = 0, yAxis = 0, x = rezItem.X, y = rezItem.K }, text = rezItem.Adi + "<br>X:" + rezItem.X + " Y:" + rezItem.K, shape = "connector", allowOverlap = true };
                    else
                        label = new AnnotationLabelsDTO { x = -20, y = -20, point = new PointDTO { xAxis = 0, yAxis = 0, x = rezItem.X, y = rezItem.K }, text = rezItem.R + " ohm<br>X:" + rezItem.X + " Y:" + rezItem.K, shape = "connector", allowOverlap = true };
                    annotations.labels.Add(label);
                }
                annotationsList.Add(annotations);
            }

            return annotationsList;
        }
        private List<AnnotationsDTO> GraphAnnotationsOlustur(List<List<SismikDTO>> sisGenelList)
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
                        label = new AnnotationLabelsDTO { x = 20, y = -20, point = new PointDTO { xAxis = 0, yAxis = 0, x = sisItem.X, y = sisItem.K }, text = sisItem.Adi + "<br>X:" + sisItem.X + " Y:" + sisItem.K, shape = "connector", allowOverlap = true };
                    else
                        label = new AnnotationLabelsDTO { x = 20, y = -20, point = new PointDTO { xAxis = 0, yAxis = 0, x = sisItem.X, y = sisItem.K }, text = "Vp = " + sisItem.Vp + "m/s<br>Vs =" + sisItem.Vs + "m/s<br>X:" + sisItem.X + " Y:" + sisItem.K, shape = "connector", allowOverlap = true };
                    annotations.labels.Add(label);
                }
                annotationsList.Add(annotations);
            }

            return annotationsList;
        }
        private List<AnnotationsDTO> GraphAnnotationsOlustur(List<List<SondajDTO>> sonGenelList)
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
                        label = new AnnotationLabelsDTO { x = 20, y = 20, point = new PointDTO { xAxis = 0, yAxis = 0, x = sonItem.X, y = sonItem.K }, text = sonItem.Adi + "<br>X:" + sonItem.X + " Y:" + sonItem.K, shape = "connector", allowOverlap = true };
                    else
                        label = new AnnotationLabelsDTO { x = 20, y = 20, point = new PointDTO { xAxis = 0, yAxis = 0, x = sonItem.X, y = sonItem.K }, text = sonItem.T + "<br>X:" + sonItem.X + " Y:" + sonItem.K, shape = "connector", allowOverlap = true };
                    annotations.labels.Add(label);
                }
                annotationsList.Add(annotations);
            }

            return annotationsList;
        }



        private List<SeriesDTO> GraphDataOlustur(List<List<RezistiviteDTO>> rezGenelList)
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
                dataset.toolTip = new ToolTipDTO { enabled = false };
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
        private List<SeriesDTO> GraphDataOlustur(List<List<SismikDTO>> sisGenelList)
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
                dataset.toolTip = new ToolTipDTO { enabled = false };
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
        private List<SeriesDTO> GraphDataOlustur(List<List<SondajDTO>> sonGenelList)
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
                dataset.toolTip = new ToolTipDTO { enabled = false };
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

        private List<SeriesDTO> GraphDataOlustur(long kuralID, KesitDTO kesitDTO)
        {
            KuralGetirDTO kuralGetir = _fuzzyManager.KuralGetir(kuralID);

            List<SeriesDTO> datasetList = new List<SeriesDTO>();
            SeriesDTO dataset;
            var name = "Set-";
            int count = 0;
            var random = new Random();
            for (int i = 0; i < kesitDTO.RezGenelList.Count; i++)
            {
                count++;
                dataset = new SeriesDTO();
                dataset.name = name + count.ToString();
                dataset.lineWidth = 2;
                dataset.color = RenkUret(i, kesitDTO.RezGenelList.Count); // String.Format("#{0:X6}", random.Next(0x1000000));
                dataset.showInLegend = false;
                dataset.marker = new MarkerDTO { symbol = "circle", radius = 2, enabled = true };
                dataset.toolTip = new ToolTipDTO { enabled = false };
                dataset.states = new StatesDTO { hover = new HoverDTO { lineWidthPlus = 3 } };
                //dataset.enableMouseTracking = false;
                dataset.draggableY = true;
                dataset.draggableX = true;
                for (int j = 0; j < kesitDTO.RezGenelList[i].Count; j++)
                {
                    List<double> coordinates = new List<double>();
                    if (i == 0) //ilk çizim daima yapılacak
                    {
                        coordinates.Add(kesitDTO.RezGenelList[i][j].X);
                        coordinates.Add((double)kesitDTO.RezGenelList[i][j].K);
                        dataset.data.Add(coordinates);
                        continue;
                    }
                    if (j != kesitDTO.RezGenelList[i].Count - 1) //Son sıra kontrolü
                    {
                        if (kesitDTO.RezGenelList[i][j].R != null && kesitDTO.RezGenelList[i][j + 1].R != null && kesitDTO.RezGenelList[i][j].R != 0 && kesitDTO.RezGenelList[i][j + 1].R != 0)
                        {
                            var ilkDugum = _fuzzyManager.FuzzyKuralOlusturVeSonucGetirFLL(kuralGetir, (double)kesitDTO.RezGenelList[i][j].R);
                            var ikinciDugum = _fuzzyManager.FuzzyKuralOlusturVeSonucGetirFLL(kuralGetir, (double)kesitDTO.RezGenelList[i][j + 1].R);

                            if (ilkDugum == ikinciDugum) //iki özdirenç değeri de aynı aralıktaysa bu sefer hız değerlerine bakılır
                            {
                                bool VpUygunMu = SismikKontroluVp(kesitDTO, i, j);
                                bool VsUygunMu = SismikKontroluVs(kesitDTO, i, j);
                                if (VpUygunMu && VsUygunMu) //Vp Vs ve Özdirenç değerleri uygunsa birleştirme yapılır
                                {
                                    coordinates.Add(kesitDTO.RezGenelList[i][j].X);
                                    coordinates.Add((double)kesitDTO.RezGenelList[i][j].K);
                                    dataset.data.Add(coordinates);
                                }
                                else //özdirenç değerleri uygun ama sismik değerleri değil. çukur ve fay kontrolü yapılır
                                {
                                    if (j == 0)
                                    {
                                        //Fay oluştur
                                        var fayDataset = FayOlustur(dataset, kesitDTO.RezGenelList, i, j);
                                        datasetList.Add(fayDataset);
                                        continue;
                                    }
                                    else
                                    {
                                        var birOncekiDugum = _fuzzyManager.FuzzyKuralOlusturVeSonucGetirFLL(kuralGetir, (double)kesitDTO.RezGenelList[i][j - 1].R);
                                        if (birOncekiDugum == ikinciDugum)
                                        {
                                            //Çukur oluştur
                                            var cukurDataset = CukurOlustur(dataset, kesitDTO.RezGenelList, i, j);
                                            datasetList.Add(cukurDataset);
                                            continue;
                                        }
                                        else
                                        {
                                            //Fay oluştur
                                            var fayDataset = FayOlustur(dataset, kesitDTO.RezGenelList, i, j);
                                            datasetList.Add(fayDataset);
                                            continue;
                                        }
                                    }
                                }
                            }
                            else //özdirenç değerleri uygun değil. çukur ve fay kontrolü yapılır
                            {
                                if (j == 0)
                                {
                                    //Fay oluştur
                                    var fayDataset = FayOlustur(dataset, kesitDTO.RezGenelList, i, j);
                                    datasetList.Add(fayDataset);
                                    continue;
                                }
                                else
                                {
                                    var birOncekiDugum = _fuzzyManager.FuzzyKuralOlusturVeSonucGetirFLL(kuralGetir, (double)kesitDTO.RezGenelList[i][j - 1].R);
                                    if (birOncekiDugum == ikinciDugum)
                                    {
                                        //Çukur oluştur
                                        var cukurDataset = CukurOlustur(dataset, kesitDTO.RezGenelList, i, j);
                                        datasetList.Add(cukurDataset);
                                        continue;
                                    }
                                    else
                                    {
                                        //Fay oluştur
                                        var fayDataset = FayOlustur(dataset, kesitDTO.RezGenelList, i, j);
                                        datasetList.Add(fayDataset);
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        coordinates.Add(kesitDTO.RezGenelList[i][j].X);
                        coordinates.Add((double)kesitDTO.RezGenelList[i][j].K);
                        dataset.data.Add(coordinates);
                    }
                }
                datasetList.Add(dataset);
            }
            return datasetList;
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

        public SonucDTO KumeListesiGetir()
        {
            SonucDTO sonuc = new SonucDTO();
            try
            {
                var kuralList = _kuralService.Queryable().Where(k => k.AktifMi == true).Select(k => new KuralEntityDTO
                {
                    KuralID = k.KuralID,
                    KuralAdi = k.KuralAdi,
                    EklenmeTarihi = k.EklenmeTarihi,
                    AktifMi = k.AktifMi
                }).ToList();
                sonuc.Nesne = kuralList;
                sonuc.Sonuc = true;
                sonuc.Mesaj = "Başarılı.";
                return sonuc;
            }
            catch (Exception ex)
            {

                sonuc.Nesne = null;
                sonuc.Sonuc = false;
                sonuc.Mesaj = "Başarısız.";
                sonuc.Exception = ex;
                return sonuc;
            }
        }

        public SonucDTO KuralGetir(long kuralID)
        {
            SonucDTO sonuc = new SonucDTO();
            try
            {
                var kuralList = _kuralListTextService.Queryable().Where(k => k.KuralID == kuralID).Select(k => new KuralTextEntityDTO
                {
                    KuralID = k.KuralID,
                    KuralText = k.KuralText
                }).ToList();
                sonuc.Nesne = kuralList;
                sonuc.Sonuc = true;
                sonuc.Mesaj = "Başarılı.";
                return sonuc;
            }
            catch (Exception ex)
            {

                sonuc.Nesne = null;
                sonuc.Sonuc = false;
                sonuc.Mesaj = "Başarısız.";
                sonuc.Exception = ex;
                return sonuc;
            }
        }

        private bool SismikKontroluVp(KesitDTO kesitDTO, int i, int j)
        {
            if (i < kesitDTO.SisGenelList.Count && j < kesitDTO.SisGenelList[i].Count)
            {
                if (kesitDTO.SisGenelList[i][j].Vp != null && kesitDTO.SisGenelList[i][j].Vp != 0 && kesitDTO.SisGenelList[i][j].Vs != null && kesitDTO.SisGenelList[i][j].Vs != 0)
                {
                    if ((i + 1) < kesitDTO.SisGenelList.Count)
                    {
                        if ((kesitDTO.SisGenelList[i][j].X > kesitDTO.RezGenelList[i][j].X && kesitDTO.SisGenelList[i][j].X < kesitDTO.RezGenelList[i + 1][j].X) && (kesitDTO.SisGenelList[i + 1][j].X > kesitDTO.RezGenelList[i][j].X && kesitDTO.SisGenelList[i + 1][j].X < kesitDTO.RezGenelList[i + 1][j].X)) //iki özdirenç arasında birden fazla sismik ölçüm olma durumu
                        {
                            if (kesitDTO.SisGenelList[i][j].Vp > kesitDTO.SisGenelList[i + 1][j].Vp)//soldaki Vp daha büyükse
                            {
                                if (kesitDTO.SisGenelList[i][j].Vp * 0.7 > kesitDTO.SisGenelList[i + 1][j].Vp) //öncekinin %60 bir sonrakinden büyük olmalı
                                {
                                    return false;
                                }
                            }
                            else //sağdaki daha büyükse
                            {
                                if (kesitDTO.SisGenelList[i + 1][j].Vp * 0.7 > kesitDTO.SisGenelList[i][j].Vp)
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

        private bool SismikKontroluVs(KesitDTO kesitDTO, int i, int j)
        {
            if (i < kesitDTO.SisGenelList.Count && j < kesitDTO.SisGenelList[i].Count)
            {
                if (kesitDTO.SisGenelList[i][j].Vs != null && kesitDTO.SisGenelList[i][j].Vs != 0 && kesitDTO.SisGenelList[i][j].Vs != null && kesitDTO.SisGenelList[i][j].Vs != 0)
                {
                    if ((i + 1) < kesitDTO.SisGenelList.Count)
                    {
                        if ((kesitDTO.SisGenelList[i][j].X > kesitDTO.RezGenelList[i][j].X && kesitDTO.SisGenelList[i][j].X < kesitDTO.RezGenelList[i + 1][j].X) && (kesitDTO.SisGenelList[i + 1][j].X > kesitDTO.RezGenelList[i][j].X && kesitDTO.SisGenelList[i + 1][j].X < kesitDTO.RezGenelList[i + 1][j].X)) //iki özdirenç arasında birden fazla sismik ölçüm olma durumu
                        {
                            if (kesitDTO.SisGenelList[i][j].Vs > kesitDTO.SisGenelList[i + 1][j].Vs)//soldaki Vs daha büyükse
                            {
                                if (kesitDTO.SisGenelList[i][j].Vs * 0.7 > kesitDTO.SisGenelList[i + 1][j].Vs) //öncekinin %60 bir sonrakinden büyük olmalı
                                {
                                    return false;
                                }
                            }
                            else //sağdaki daha büyükse
                            {
                                if (kesitDTO.SisGenelList[i + 1][j].Vs * 0.7 > kesitDTO.SisGenelList[i][j].Vs)
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

        private SeriesDTO CukurOlustur(SeriesDTO dataset, List<List<RezistiviteDTO>> rezGenelList, int i, int j)
        {
            SeriesDTO cukurDataset = new SeriesDTO();
            cukurDataset.name = "Fay";
            cukurDataset.lineWidth = 2;
            cukurDataset.color = dataset.color;
            cukurDataset.showInLegend = false;
            cukurDataset.marker = new MarkerDTO { enabled = false };
            cukurDataset.toolTip = new ToolTipDTO { enabled = false };
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

        private SeriesDTO FayOlustur(SeriesDTO dataset, List<List<RezistiviteDTO>> rezGenelList, int i, int j)
        {
            SeriesDTO fayDataset = new SeriesDTO();
            fayDataset.name = "Fay";
            fayDataset.lineWidth = 2;
            fayDataset.color = dataset.color;
            fayDataset.showInLegend = false;
            //fayDataset.marker = new MarkerDTO { enabled = false };
            fayDataset.marker = new MarkerDTO { symbol = "circle", radius = 2, enabled = true };
            fayDataset.toolTip = new ToolTipDTO { enabled = false };
            fayDataset.states = new StatesDTO { hover = new HoverDTO { lineWidthPlus = 3 } };
            //fayDataset.enableMouseTracking = false;
            fayDataset.draggableY = true;
            fayDataset.draggableX = true;

            var ortaNoktaX = (rezGenelList[i][j].X + rezGenelList[i][j + 1].X) / 2;
            var ortaNoktaK = (rezGenelList[i][j].K + rezGenelList[i][j + 1].K) / 2;

            List<double> coordinates = new List<double>();
            coordinates.Add(rezGenelList[i][j].X);
            coordinates.Add((double)rezGenelList[i][j].K);
            dataset.data.Add(coordinates);

            coordinates = new List<double>();
            coordinates.Add(ortaNoktaX);
            coordinates.Add((double)ortaNoktaK);
            dataset.data.Add(coordinates);

            coordinates = new List<double>();
            coordinates.Add(rezGenelList[i][j + 1].X);
            coordinates.Add((double)rezGenelList[i][j + 1].K);
            dataset.data.Add(coordinates);

            //coordinates = new List<double>();
            //coordinates.Add(ortaNoktaX);
            //coordinates.Add((double)ortaNoktaK);
            //dataset.data.Add(coordinates);

            coordinates = new List<double>();
            coordinates.Add(ortaNoktaX - 0.5);
            coordinates.Add((double)ortaNoktaK - 1);
            fayDataset.data.Add(coordinates);

            //coordinates = new List<double>();
            //coordinates.Add(ortaNoktaX);
            //coordinates.Add((double)ortaNoktaK);
            //dataset.data.Add(coordinates);

            coordinates = new List<double>();
            coordinates.Add(ortaNoktaX + 0.5);
            coordinates.Add((double)ortaNoktaK + 1);
            fayDataset.data.Add(coordinates);

            //coordinates = new List<double>();
            //coordinates.Add(ortaNoktaX);
            //coordinates.Add((double)ortaNoktaK);
            //dataset.data.Add(coordinates);

            return fayDataset;
        }
    }

    public interface IGraphManager : IBaseManager
    {
        SonucDTO ExcelKontrolEt(ExcelModelDTO excel, string path);
        SonucDTO GraphOlustur(GraphDTO graph, string path);
        SonucDTO KumeListesiGetir();
        SonucDTO KuralGetir(long kuralID);
    }
}
