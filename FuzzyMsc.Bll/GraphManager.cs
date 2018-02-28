using FuzzyMsc.Bll.Interface;
using FuzzyMsc.Dto;
using FuzzyMsc.Dto.CizimDTOS;
using FuzzyMsc.Dto.FuzzyDTOS;
using FuzzyMsc.Pattern.UnitOfWork;
using FuzzyMsc.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        public GraphManager(
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

        public SonucDTO ExcelKontrolEt(ExcelModelDTO excel, string path)
        {
            File.WriteAllBytes(path, Convert.FromBase64String(excel.data));

            return null;


        }

        public SonucDTO GraphOlustur(GraphDTO graph, string path)
        {
            SonucDTO sonuc = new SonucDTO();
            List<List<RezistiviteDTO>> rezGenelList = new List<List<RezistiviteDTO>>();
            List<RezistiviteDTO> rezList = new List<RezistiviteDTO>();
            RezistiviteDTO rezItem = new RezistiviteDTO();
            File.WriteAllBytes(path, Convert.FromBase64String(graph.excel.data));
            Microsoft.Office.Interop.Excel.Application xl = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel.Workbook xlWorkbook = xl.Workbooks.Open(path);

            #region Rezistivite
            Microsoft.Office.Interop.Excel._Worksheet xlWorksheetRezistivite = (Microsoft.Office.Interop.Excel._Worksheet)xlWorkbook.Sheets[1];
            Microsoft.Office.Interop.Excel.Range xlRange = xlWorksheetRezistivite.UsedRange;
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
                        rezItem.K = (double)(xlWorksheetRezistivite.Cells[i + 1, 4]).Value - ((double)(xlWorksheetRezistivite.Cells[i + 1, 4]).Value * (80 / 100));
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
                count = 0;
            }

            List<GraphDatasetDTO> rezData = GraphDataOlustur(rezGenelList);
            #endregion

            #region Sismik
            Microsoft.Office.Interop.Excel._Worksheet xlWorksheetSismik = (Microsoft.Office.Interop.Excel._Worksheet)xlWorkbook.Sheets[2];
            #endregion

            #region Sondaj
            Microsoft.Office.Interop.Excel._Worksheet xlWorksheetSondaj = (Microsoft.Office.Interop.Excel._Worksheet)xlWorkbook.Sheets[3];
            #endregion
            //Microsoft.Office.Interop.Excel.Range xlRange = xlWorksheet.UsedRange;

            //if (File.Exists(path))
            //    File.Delete(path);
            sonuc.Nesne = rezData;
            sonuc.Sonuc = true;
            return sonuc;
        }

        private List<GraphDatasetDTO> GraphDataOlustur(List<List<RezistiviteDTO>> rezGenelList)
        {
            List<GraphDatasetDTO> datasetList = new List<GraphDatasetDTO>();
            GraphDatasetDTO dataset;
            var name = "Set-";
            int count = 0;
            var random = new Random();
            foreach (var rezList in rezGenelList)
            {
                count++;
                dataset = new GraphDatasetDTO();
                dataset.name = name + count.ToString();
                dataset.lineWidth = 0;
                dataset.color = String.Format("#{0:X6}", random.Next(0x1000000));
                dataset.showInLegend = true;
                dataset.marker = new MarkerDTO { symbol = "circle",radius = 2, enabled=true };
                dataset.toolTip = new ToolTipDTO { headerFormat = "<b>{series.name}</b><br>", pointFormat= "{point.x:.2f}: {point.y:.2f}" };
                dataset.states = new StatesDTO { hover = new HoverDTO { lineWidthPlus = 0 } };
                foreach (var rezItem in rezList)
                {
                    List<double> coordinates = new List<double>();
                    coordinates.Add((double)rezItem.X);
                    coordinates.Add((double)rezItem.K);
                    dataset.data.Add(coordinates);
                }
                datasetList.Add(dataset);
            }
            return datasetList;
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
                var kuralList = _kuralListTextService.Queryable().Select(k => new KuralTextEntityDTO
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
    }

    public interface IGraphManager : IBaseManager
    {
        SonucDTO ExcelKontrolEt(ExcelModelDTO excel, string path);
        SonucDTO GraphOlustur(GraphDTO graph, string path);
        SonucDTO KumeListesiGetir();
        SonucDTO KuralGetir(long kuralID);
    }
}
