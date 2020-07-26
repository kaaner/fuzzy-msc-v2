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

		private List<List<ResistivityDTO>> resGeneralList;
		private List<List<SeismicDTO>> seisGeneralList;
		private List<List<DrillDTO>> driGeneralList;
		private GraphCountDTO graphCount = new GraphCountDTO();
		private List<GraphDetailedDTO> graphDetailedList = new List<GraphDetailedDTO>();
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

		public ResultDTO CheckExcel(ExcelModelDTO excel, string path)
		{
			ResultDTO result = new ResultDTO();
			try
			{
				result.Result = true;
				File.WriteAllBytes(path, Convert.FromBase64String(excel.data));
			}
			catch (Exception ex)
			{
				result.Result = false;
			}
			return result;


		}
		public ResultDTO GenerateGraph(GraphDTO graph, string path)
		{
			try
			{
				ResultDTO result = new ResultDTO();
				File.WriteAllBytes(path, Convert.FromBase64String(graph.excel.data));
				xl = new Microsoft.Office.Interop.Excel.Application();
				xlWorkbook = xl.Workbooks.Open(path);
				GetWindowThreadProcessId(xl.Hwnd, out id);
				HighchartsDTO highcharts = new HighchartsDTO();

				#region Resistivity
				GenerateResistivity(highcharts, xlWorkbook);
				#endregion

				#region Seismic
				GenerateSeismic(highcharts, xlWorkbook);
				#endregion

				#region Drill
				GenerateDrill(highcharts, xlWorkbook);
				#endregion

				//highcharts.series.AddRange(GraphDataOlustur(sisGenelList));
				//highcharts.series.AddRange(GraphDataOlustur(sonGenelList));
				//highcharts.series.AddRange(GraphDataOlustur(rezGenelList));
				SectionDTO sectionDTO = new SectionDTO { ResistivityGeneralList = resGeneralList, SeismicGeneralList = seisGeneralList, DrillGeneralList = driGeneralList };
				highcharts.series.AddRange(GenerateGraphData(graph.ruleId, sectionDTO, graph.parameters));

				bool hasFault = highcharts.series.Any(s => s.name == "Fault (Fay)");

				if (hasFault)
					highcharts.series.AddRange(GenerateGraphData(graph.ruleId, sectionDTO, graph.parameters));

				highcharts.series = highcharts.series.Distinct().ToList();

				double minX = CalculateMin(highcharts);
				highcharts.xAxis = new AxisDTO { min = 0, minTickInterval = (int)graph.parameters.ScaleX, offset = 20, title = new AxisTitleDTO { text = "Genişlik" }, labels = new AxisLabelsDTO { format = "{value} m" } };
				highcharts.yAxis = new AxisDTO { min = (int)minX - 5, minTickInterval = (int)graph.parameters.ScaleY, offset = 20, title = new AxisTitleDTO { text = "Yükseklik" }, labels = new AxisLabelsDTO { format = "{value} m" } };

				highcharts.parameters = graph.parameters;
				highcharts.graphInfo = graphDetailedList;

				result.Object = highcharts;
				result.Result = true;
				return result;
			}
			finally
			{
				xlWorkbook.Close();
				xl.Quit();
				Process process = Process.GetProcessById(id);
				process.Kill();

			}
		}

		private double CalculateSuccess(GraphCountDTO graphCount, GraphCountDTO defaultCount)
		{
			double rate = 100.0;

			int normalDifference = Math.Abs(graphCount.Normal - defaultCount.Normal);
			int pocketDifference = Math.Abs(graphCount.Pocket - defaultCount.Pocket);
			int faultDifference = Math.Abs(graphCount.Fault - defaultCount.Fault);

			for (int i = 0; i < faultDifference; i++)
			{
				rate = rate - (rate * 15 / 100);
			}
			for (int i = 0; i < pocketDifference; i++)
			{
				rate = rate - (rate * 7.5 / 100);
			}
			for (int i = 0; i < normalDifference; i++)
			{
				rate = rate - (rate * 1 / 100);
			}

			rate = rate - (rate * 1 / 100); //%1 deflection rate cause of drawings (%1 oranında çizimlerden kaynaklı sapma)

			rate = rate - (rate * (normalDifference + pocketDifference + faultDifference) / 100); //General deflection cause of difference of figure count (Şekil sayılarının farkına göre genel sapma)

			return rate;
		}

		private double CalculateMin(HighchartsDTO highcharts)
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

		private void GenerateResistivity(HighchartsDTO highcharts, Workbook xlWorkbook)
		{
			resGeneralList = new List<List<ResistivityDTO>>();
			List<ResistivityDTO> resList = new List<ResistivityDTO>();
			ResistivityDTO resItem = new ResistivityDTO();
			Microsoft.Office.Interop.Excel._Worksheet xlWorksheetResistivity = (Microsoft.Office.Interop.Excel._Worksheet)xlWorkbook.Sheets[1];
			Microsoft.Office.Interop.Excel.Range xlRange = xlWorksheetResistivity.UsedRange;
			#region Table Row and Column Widths (Tablo Satir ve Sutun Genislikleri)
			int rowCount = xlRange.Rows.Count;
			int colCount = xlRange.Columns.Count;
			for (int i = 1; i <= rowCount; i++)
			{
				if (string.IsNullOrEmpty((xlWorksheetResistivity.Cells[i + 1, 1]).Value))
				{
					rowCount = i;
					break;
				}
			}
			#endregion

			#region Sync of Depth (Derinlik Eşitleme)

			List<List<ExcelDTO>> resExcel = new List<List<ExcelDTO>>();
			List<ExcelDTO> resExcelItem;
			#region All data is being transferred (Tüm Veriler Aktarılıyor)
			for (int i = 2; i < rowCount + 1; i++)
			{
				resExcelItem = new List<ExcelDTO>();
				for (int j = 1; j < colCount + 1; j++)
				{
					ExcelDTO Instance;
					if ((xlWorksheetResistivity.Cells[i, j]).Value == null) //Boş olan hücrelerde hata verdiği için kontrol yapılıyor
					{
						Instance = new ExcelDTO { TypeID = (byte)Enums.ExcelDataType.Real, Value = "" };
						resExcelItem.Add(Instance);
					}
					else
					{
						var value = (string)(xlWorksheetResistivity.Cells[i, j]).Value.ToString();
						Instance = new ExcelDTO { TypeID = (byte)Enums.ExcelDataType.Real, Value = value };
						resExcelItem.Add(Instance);
					}

				}
				resExcel.Add(resExcelItem);
			}
			#endregion

			#region Artificial Data is Added by Slipping (Kaydırmalar Yapılarak Yapay Veriler Ekleniyor)
			foreach (var item in resExcel)
			{
				if (item[item.Count - 1].Value == "" && item[item.Count - 2].Value == "") //If Last Two Cells Are Empty, Then Slip (Son İki Hücre Boş İse Kaydırma Yapılacak)
				{
					for (int i = 0; i < item.Count; i++)
					{
						if (item[i].Value == "" && item[i + 1].Value == "")//Find the two cells empty in a row (İlk Boşluk Olan İkili Hücre bulunuyor)
						{
							item[i - 2].JSONData = JsonConvert.SerializeObject(item[i - 2]);
							item[i - 1].JSONData = JsonConvert.SerializeObject(item[i - 1]);

							List<ExcelDTO> finalItem = new List<ExcelDTO>();//Values to be put to the end (Sona atılacak değerler)
							finalItem.Add(JsonConvert.DeserializeObject<ExcelDTO>(item[i - 2].JSONData));
							finalItem.Add(JsonConvert.DeserializeObject<ExcelDTO>(item[i - 1].JSONData));

							for (int j = i; j < item.Count; j = j + 2)//Slip from last cell which is not empty to the end (İçi dolu olan son hücreden itibaren sona kadar kaydırma yapılıyor)
							{
								item[j - 2].JSONData = JsonConvert.SerializeObject(item[i - 4]);
								item[j - 1].JSONData = JsonConvert.SerializeObject(item[i - 3]);
								item[j - 2] = JsonConvert.DeserializeObject<ExcelDTO>(item[j - 2].JSONData);
								item[j - 1] = JsonConvert.DeserializeObject<ExcelDTO>(item[j - 1].JSONData);
								item[j - 4].TypeID = (byte)Enums.ExcelDataType.Artificial;
								item[j - 3].TypeID = (byte)Enums.ExcelDataType.Artificial;

								if (j == item.Count - 2)
								{
									item[j].JSONData = JsonConvert.SerializeObject(finalItem[0]); //Slipping of the last values (Son değerlerin kaydırılması)
									item[j + 1].JSONData = JsonConvert.SerializeObject(finalItem[1]);
									item[j] = JsonConvert.DeserializeObject<ExcelDTO>(item[j].JSONData);
									item[j + 1] = JsonConvert.DeserializeObject<ExcelDTO>(item[j + 1].JSONData);
									item[j - 2].TypeID = (byte)Enums.ExcelDataType.Real;//Make real values which before the last one (Sondan önceki değerlerin gerçek hale getirilmesi)
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
						
			for (int i = 0; i < rowCount - 1; i++)
			{
				resItem = new ResistivityDTO();
				resItem.ID = i + 1;
				resItem.Name = resExcel[i][0].Value.ToString();
				resItem.X = Convert.ToDouble(resExcel[i][1].Value);
				resItem.K = Convert.ToDouble(resExcel[i][3].Value);
				resItem.TypeID = resExcel[i][0].TypeID;
				resList.Add(resItem);
			}
			resGeneralList.Add(resList);

			int count = 0;
			for (int j = 4; j < colCount; j = j + 2)
			{
				count++;
				resList = new List<ResistivityDTO>();
				for (int i = 0; i < rowCount - 1; i++)
				{
					var resExcelInstance = resExcel[i];

					if (resExcelInstance[j].Value == "" && resExcelInstance[j + 1].Value == "") //If both values are empty (coordinate and resistivity) (Exceldeki İki Hücre değeri de boşsa (hem koordinat hem de özdirenç))
					{
						continue;
					}
					if (resExcelInstance[j + 1].Value == "")
					{
						continue;
					}
					if (resExcelInstance[j].Value == "" && resExcelInstance[j + 1].Value != "")//If only the depth value is empty (Sadece Derinlik Değeri Boşsa)
					{
						resItem = new ResistivityDTO();
						resItem.ID = i + 1;
						resItem.Name = resExcelInstance[0].Value.ToString() + count.ToString();
						resItem.X = Convert.ToDouble(resExcelInstance[1].Value);
						var value = "";
						for (int k = 0; k < resExcelInstance.Count; k = k + 2)
						{
							if (resExcelInstance[j - (2 + k)].TypeID == (byte)Enums.ExcelDataType.Real)
							{
								value = resExcelInstance[j - (2 + k)].Value;
								break;
							}
						}
						resItem.K = (Convert.ToDouble(resExcelInstance[3].Value) - Convert.ToDouble(value)) * 0.99;
						resItem.R = resExcelInstance[j + 1].Value == "" ? Convert.ToDouble("") : Convert.ToDouble(resExcelInstance[j + 1].Value);
						resItem.TypeID = resExcelInstance[j].TypeID;
						resList.Add(resItem);
						continue;
					}
					resItem = new ResistivityDTO();
					resItem.ID = i + 1;
					resItem.Name = resExcelInstance[0].Value.ToString() + count.ToString();
					resItem.X = Convert.ToDouble(resExcelInstance[1].Value);
					resItem.K = resExcelInstance[j].Value == "" ? 0 : Convert.ToDouble(resExcelInstance[3].Value) - Convert.ToDouble(resExcelInstance[j].Value);
					resItem.R = resExcelInstance[j + 1].Value == "" ? Convert.ToDouble("") : Convert.ToDouble(resExcelInstance[j + 1].Value);
					resItem.TypeID = resExcelInstance[j].TypeID;
					resList.Add(resItem);

				}
				resGeneralList.Add(resList);
			}

			highcharts = GenerateChart(highcharts, resGeneralList);
		}
		private void GenerateSeismic(HighchartsDTO highcharts, Workbook xlWorkbook)
		{
			seisGeneralList = new List<List<SeismicDTO>>();
			List<SeismicDTO> seisList = new List<SeismicDTO>();
			SeismicDTO seisItem = new SeismicDTO();
			Microsoft.Office.Interop.Excel._Worksheet xlWorksheetSeismic = (Microsoft.Office.Interop.Excel._Worksheet)xlWorkbook.Sheets[2];
			Microsoft.Office.Interop.Excel.Range xlRange = xlWorksheetSeismic.UsedRange;
			#region Table Row and Column Widths (Tablo Satir ve Sutun Genislikleri)
			int rowCount = xlRange.Rows.Count;
			int colCount = xlRange.Columns.Count;
			for (int i = 1; i <= rowCount; i++)
			{
				if (string.IsNullOrEmpty((xlWorksheetSeismic.Cells[i + 1, 1]).Value))
				{
					rowCount = i;
					break;
				}
			}
			#endregion

			#region Sync of Depth (Derinlik Eşitleme)

			List<List<ExcelDTO>> seisExcel = new List<List<ExcelDTO>>();
			List<ExcelDTO> seisExcelItem;
			#region All data is being transferred (Tüm Veriler Aktarılıyor)
			for (int i = 2; i < rowCount + 1; i++)
			{
				seisExcelItem = new List<ExcelDTO>();
				for (int j = 1; j < colCount + 1; j++)
				{
					ExcelDTO Instance;
					if ((xlWorksheetSeismic.Cells[i, j]).Value == null) //Checking for empty cells because of errors (Boş olan hücrelerde hata verdiği için kontrol yapılıyor)
					{
						Instance = new ExcelDTO { TypeID = (byte)Enums.ExcelDataType.Real, Value = "" };
						seisExcelItem.Add(Instance);
					}
					else
					{
						var value = (string)(xlWorksheetSeismic.Cells[i, j]).Value.ToString();
						Instance = new ExcelDTO { TypeID = (byte)Enums.ExcelDataType.Real, Value = value };
						seisExcelItem.Add(Instance);
					}

				}
				seisExcel.Add(seisExcelItem);
			}
			#endregion

			#region Artificial Data is Added by Slipping (Kaydırmalar Yapılarak Yapay Veriler Ekleniyor)
			foreach (var item in seisExcel)
			{
				if (item[item.Count - 1].Value == "" && item[item.Count - 2].Value == "" && item[item.Count - 3].Value == "") //If Last Two Cells Are Empty, Then Slip (Son İki Hücre Boş İse Kaydırma Yapılacak)
				{
					for (int i = 0; i < item.Count; i++)
					{
						if (item[i].Value == "" && item[i + 1].Value == "" && item[i + 2].Value == "")//Find the two cells empty in a row (İlk Boşluk Olan İkili Hücre bulunuyor)
						{
							item[i - 3].JSONData = JsonConvert.SerializeObject(item[i - 3]);
							item[i - 2].JSONData = JsonConvert.SerializeObject(item[i - 2]);
							item[i - 1].JSONData = JsonConvert.SerializeObject(item[i - 1]);

							List<ExcelDTO> finalItem = new List<ExcelDTO>();//Values to be put to the end (Sona atılacak değerler)
							finalItem.Add(JsonConvert.DeserializeObject<ExcelDTO>(item[i - 3].JSONData));
							finalItem.Add(JsonConvert.DeserializeObject<ExcelDTO>(item[i - 2].JSONData));
							finalItem.Add(JsonConvert.DeserializeObject<ExcelDTO>(item[i - 1].JSONData));

							for (int j = i; j < item.Count; j = j + 3)//Slip from last cell which is not empty to the end (İçi dolu olan son hücreden itibaren sona kadar kaydırma yapılıyor)
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
									item[j].JSONData = JsonConvert.SerializeObject(finalItem[0]);//Slipping of the last values (Son değerlerin kaydırılması)
									item[j + 1].JSONData = JsonConvert.SerializeObject(finalItem[1]);
									item[j + 2].JSONData = JsonConvert.SerializeObject(finalItem[2]);
									item[j] = JsonConvert.DeserializeObject<ExcelDTO>(item[j].JSONData);
									item[j + 1] = JsonConvert.DeserializeObject<ExcelDTO>(item[j + 1].JSONData);
									item[j + 2] = JsonConvert.DeserializeObject<ExcelDTO>(item[j + 2].JSONData);
									item[j - 3].TypeID = (byte)Enums.ExcelDataType.Real;//Make real values which before the last one (Sondan önceki değerlerin gerçek hale getirilmesi)
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

			for (int i = 0; i < rowCount - 1; i++)
			{
				seisItem = new SeismicDTO();
				seisItem.ID = i + 1;
				seisItem.Name = seisExcel[i][0].Value.ToString();
				seisItem.X = Convert.ToDouble(seisExcel[i][1].Value);
				seisItem.K = Convert.ToDouble(seisExcel[i][3].Value);
				seisList.Add(seisItem);
			}
			seisGeneralList.Add(seisList);

			int count = 0;
			for (int j = 4; j < colCount; j = j + 3)
			{
				count++;
				seisList = new List<SeismicDTO>();
				for (int i = 0; i < rowCount - 1; i++)
				{
					var sisExcelInstance = seisExcel[i];
					if (sisExcelInstance[j].TypeID == (byte)Enums.ExcelDataType.Real)
					{
						if (sisExcelInstance[j].Value == "" && sisExcelInstance[j + 1].Value == "" && sisExcelInstance[j + 2].Value == "")
						{
							continue;
						}
						if (sisExcelInstance[j].Value == "" && sisExcelInstance[j + 1].Value != "" && sisExcelInstance[j + 2].Value != "")
						{
							seisItem = new SeismicDTO();
							seisItem.ID = i + 1;
							seisItem.Name = sisExcelInstance[0].Value.ToString() + count.ToString();
							seisItem.X = Convert.ToDouble(sisExcelInstance[1].Value);
							var value = "";
							for (int k = 0; k < sisExcelInstance.Count; k = k + 3)
							{
								if (sisExcelInstance[j - (3 + k)].TypeID == (byte)Enums.ExcelDataType.Real)
								{
									value = sisExcelInstance[j - (3 + k)].Value;
									break;
								}
							}
							seisItem.K = (Convert.ToDouble(sisExcelInstance[3].Value) - Convert.ToDouble(value)) * 0.99;
							seisItem.Vp = sisExcelInstance[j + 1].Value == "" ? Convert.ToDouble("") : Convert.ToDouble(sisExcelInstance[j + 1].Value);
							seisItem.Vs = sisExcelInstance[j + 2].Value == "" ? Convert.ToDouble("") : Convert.ToDouble(sisExcelInstance[j + 2].Value);
							seisList.Add(seisItem);
							continue;
						}
						seisItem = new SeismicDTO();
						seisItem.ID = i + 1;
						seisItem.Name = sisExcelInstance[0].Value.ToString() + count.ToString();
						seisItem.X = Convert.ToDouble(sisExcelInstance[1].Value);
						seisItem.K = sisExcelInstance[j].Value == "" ? 0 : Convert.ToDouble(sisExcelInstance[3].Value) - Convert.ToDouble(sisExcelInstance[j].Value);
						seisItem.Vp = sisExcelInstance[j + 1].Value == "" ? Convert.ToDouble("") : Convert.ToDouble(sisExcelInstance[j + 1].Value);
						seisItem.Vs = sisExcelInstance[j + 2].Value == "" ? Convert.ToDouble("") : Convert.ToDouble(sisExcelInstance[j + 2].Value);
						seisList.Add(seisItem);
					}
				}
				seisGeneralList.Add(seisList);
			}

			highcharts = GenerateChart(highcharts, seisGeneralList);
		}
		private void GenerateDrill(HighchartsDTO highcharts, Workbook xlWorkbook)
		{
			driGeneralList = new List<List<DrillDTO>>();
			List<DrillDTO> drillList = new List<DrillDTO>();
			DrillDTO drillItem = new DrillDTO();
			Microsoft.Office.Interop.Excel._Worksheet xlWorkSheetDrill = (Microsoft.Office.Interop.Excel._Worksheet)xlWorkbook.Sheets[3];
			Microsoft.Office.Interop.Excel.Range xlRange = xlWorkSheetDrill.UsedRange;
			#region Table Row and Column Widths (Tablo Satir ve Sutun Genislikleri)
			int rowCount = xlRange.Rows.Count;
			int colCount = xlRange.Columns.Count;
			for (int i = 1; i <= rowCount; i++)
			{
				if (string.IsNullOrEmpty((xlWorkSheetDrill.Cells[i + 1, 1]).Value))
				{
					rowCount = i;
					break;
				}
			}
			#endregion

			for (int i = 1; i < rowCount; i++)
			{
				drillItem = new DrillDTO();
				drillItem.ID = i;
				drillItem.Name = (string)(xlWorkSheetDrill.Cells[i + 1, 1]).Value.ToString();
				drillItem.X = (double)(xlWorkSheetDrill.Cells[i + 1, 2]).Value;
				drillItem.K = (double)(xlWorkSheetDrill.Cells[i + 1, 4]).Value;
				drillList.Add(drillItem);
			}
			driGeneralList.Add(drillList);

			int count = 0;
			for (int j = 5; j <= colCount; j = j + 2)
			{
				count++;
				drillList = new List<DrillDTO>();
				for (int i = 1; i <= rowCount; i++)
				{

					drillItem = new DrillDTO();
					if ((xlWorkSheetDrill.Cells[i + 1, j]).Value == null && (xlWorkSheetDrill.Cells[i + 1, j + 1]).Value == null)
					{
						continue;
					}
					if ((xlWorkSheetDrill.Cells[i + 1, j]).Value == null && (xlWorkSheetDrill.Cells[i + 1, j + 1]).Value != null)
					{
						drillItem.ID = i;
						drillItem.Name = (string)(xlWorkSheetDrill.Cells[i + 1, 1]).Value.ToString() + count.ToString();
						drillItem.X = (double)(xlWorkSheetDrill.Cells[i + 1, 2]).Value;
						drillItem.K = ((double)(xlWorkSheetDrill.Cells[i + 1, 4]).Value - (double)(xlWorkSheetDrill.Cells[i + 1, j - 2]).Value) * 0.99;
						drillItem.T = (xlWorkSheetDrill.Cells[i + 1, j + 1]).Value == null ? "" : (xlWorkSheetDrill.Cells[i + 1, j + 1]).Value;
						drillList.Add(drillItem);
						continue;
					}
					drillItem.ID = i;
					drillItem.Name = (string)(xlWorkSheetDrill.Cells[i + 1, 1]).Value.ToString() + count.ToString();
					drillItem.X = (double)(xlWorkSheetDrill.Cells[i + 1, 2]).Value;
					drillItem.K = (xlWorkSheetDrill.Cells[i + 1, j]).Value == null ? 0 : (double)(xlWorkSheetDrill.Cells[i + 1, 4]).Value - (double)(xlWorkSheetDrill.Cells[i + 1, j]).Value;
					drillItem.T = (xlWorkSheetDrill.Cells[i + 1, j + 1]).Value == null ? "" : ((xlWorkSheetDrill.Cells[i + 1, j + 1]).Value).ToString();
					drillList.Add(drillItem);
				}
				driGeneralList.Add(drillList);
			}

			highcharts = GenerateChart(highcharts, driGeneralList);
		}

		private HighchartsDTO GenerateChart(HighchartsDTO highcharts, List<List<ResistivityDTO>> resGeneralList)
		{
			//highcharts.series.AddRange(GraphDataOlustur(rezGenelList));
			highcharts.annotations.AddRange(GenerateGraphAnnotations(resGeneralList));

			return highcharts;
		}
		private HighchartsDTO GenerateChart(HighchartsDTO highcharts, List<List<SeismicDTO>> seisGeneralList)
		{
			//highcharts.series.AddRange(GraphDataOlustur(sisGenelList));
			highcharts.annotations.AddRange(GenerateGraphAnnotations(seisGeneralList));

			return highcharts;
		}
		private HighchartsDTO GenerateChart(HighchartsDTO highcharts, List<List<DrillDTO>> driGeneralList)
		{
			//highcharts.series.AddRange(GraphDataOlustur(sonGenelList));
			highcharts.annotations.AddRange(GenerateGraphAnnotations(driGeneralList));

			return highcharts;
		}

		private List<AnnotationsDTO> GenerateGraphAnnotations(List<List<ResistivityDTO>> resGeneralList)
		{
			List<AnnotationsDTO> annotationsList = new List<AnnotationsDTO>();
			AnnotationsDTO annotations;
			AnnotationLabelsDTO label;

			for (int i = 0; i < resGeneralList.Count; i++)
			{
				annotations = new AnnotationsDTO();
				annotations.visible = true;
				//annotations.labelOptions = new AnnotationLabelOptionsDTO { shape = "connector", align = "right", justify = false, crop = true, style = new StyleDTO { fontSize = "0.8em", textOutline = "1px white" } };
				foreach (var resItem in resGeneralList[i].Where(k => k.TypeID == (byte)Enums.ExcelDataType.Real))
				{
					if (i == 0)
						label = new AnnotationLabelsDTO { x = -20, y = -20, point = new PointDTO { xAxis = 0, yAxis = 0, x = resItem.X, y = resItem.K }, text = resItem.Name, shape = "connector", allowOverlap = true };
					//label = new AnnotationLabelsDTO { x = -20, y = -20, point = new PointDTO { xAxis = 0, yAxis = 0, x = rezItem.X, y = rezItem.K }, text = rezItem.Adi + "<br>X:" + rezItem.X + " Y:" + rezItem.K, shape = "connector", allowOverlap = true };
					else
						label = new AnnotationLabelsDTO { x = -20, y = -20, point = new PointDTO { xAxis = 0, yAxis = 0, x = resItem.X, y = resItem.K }, text = resItem.R + " ohm", shape = "connector", allowOverlap = true };
					//label = new AnnotationLabelsDTO { x = -20, y = -20, point = new PointDTO { xAxis = 0, yAxis = 0, x = rezItem.X, y = rezItem.K }, text = rezItem.R + " ohm<br>X:" + rezItem.X + " Y:" + rezItem.K, shape = "connector", allowOverlap = true };
					annotations.labels.Add(label);
				}
				annotationsList.Add(annotations);
			}

			return annotationsList;
		}
		private List<AnnotationsDTO> GenerateGraphAnnotations(List<List<SeismicDTO>> seisGeneralList)
		{
			List<AnnotationsDTO> annotationsList = new List<AnnotationsDTO>();
			AnnotationsDTO annotations;
			AnnotationLabelsDTO label;

			for (int i = 0; i < seisGeneralList.Count; i++)
			{
				annotations = new AnnotationsDTO();
				annotations.visible = true;
				//annotations.labelOptions = new AnnotationLabelOptionsDTO { shape = "connector", align = "right", justify = false, crop = true, style = new StyleDTO { fontSize = "0.8em", textOutline = "1px white" } };
				foreach (var seisItem in seisGeneralList[i])
				{
					if (i == 0)
						label = new AnnotationLabelsDTO { x = 20, y = -20, point = new PointDTO { xAxis = 0, yAxis = 0, x = seisItem.X, y = seisItem.K }, text = seisItem.Name, shape = "connector", allowOverlap = true };
					//label = new AnnotationLabelsDTO { x = 20, y = -20, point = new PointDTO { xAxis = 0, yAxis = 0, x = sisItem.X, y = sisItem.K }, text = sisItem.Adi + "<br>X:" + sisItem.X + " Y:" + sisItem.K, shape = "connector", allowOverlap = true };
					else
						label = new AnnotationLabelsDTO { x = 20, y = -20, point = new PointDTO { xAxis = 0, yAxis = 0, x = seisItem.X, y = seisItem.K }, text = "Vp = " + seisItem.Vp + "m/s<br>Vs =" + seisItem.Vs + "m/s", shape = "connector", allowOverlap = true };
					//label = new AnnotationLabelsDTO { x = 20, y = -20, point = new PointDTO { xAxis = 0, yAxis = 0, x = sisItem.X, y = sisItem.K }, text = "Vp = " + sisItem.Vp + "m/s<br>Vs =" + sisItem.Vs + "m/s<br>X:" + sisItem.X + " Y:" + sisItem.K, shape = "connector", allowOverlap = true };
					annotations.labels.Add(label);
				}
				annotationsList.Add(annotations);
			}

			return annotationsList;
		}
		private List<AnnotationsDTO> GenerateGraphAnnotations(List<List<DrillDTO>> driGeneralList)
		{
			List<AnnotationsDTO> annotationsList = new List<AnnotationsDTO>();
			AnnotationsDTO annotations;
			AnnotationLabelsDTO label;

			for (int i = 0; i < driGeneralList.Count; i++)
			{
				annotations = new AnnotationsDTO();
				annotations.visible = true;
				// annotations.labelOptions = new AnnotationLabelOptionsDTO { shape = "connector", align = "right", justify = false, crop = true, style = new StyleDTO { fontSize = "0.8em", textOutline = "1px white" } };
				foreach (var drillItem in driGeneralList[i])
				{
					if (i == 0)
						//label = new AnnotationLabelsDTO { x = 20, y = 20, point = new PointDTO { xAxis = 0, yAxis = 0, x = sonItem.X, y = sonItem.K }, text = sonItem.Adi + "<br>X:" + sonItem.X + " Y:" + sonItem.K, shape = "connector", allowOverlap = true };
						label = new AnnotationLabelsDTO { x = 20, y = 20, point = new PointDTO { xAxis = 0, yAxis = 0, x = drillItem.X, y = drillItem.K }, text = drillItem.Name, shape = "connector", allowOverlap = true };
					else
						//label = new AnnotationLabelsDTO { x = 20, y = 20, point = new PointDTO { xAxis = 0, yAxis = 0, x = sonItem.X, y = sonItem.K }, text = sonItem.T + "<br>X:" + sonItem.X + " Y:" + sonItem.K, shape = "connector", allowOverlap = true };
						label = new AnnotationLabelsDTO { x = 20, y = 20, point = new PointDTO { xAxis = 0, yAxis = 0, x = drillItem.X, y = drillItem.K }, text = drillItem.T, shape = "connector", allowOverlap = true };
					annotations.labels.Add(label);
				}
				annotationsList.Add(annotations);
			}

			return annotationsList;
		}

		public List<SeriesDTO> GenerateGraphData(long ruleId, SectionDTO sectionDTO, ParametersDTO parameters)
		{
			GetRuleDTO getRule = _fuzzyManager.GetRule(ruleId);
			GraphDetailedDTO graphDetailed = new GraphDetailedDTO();

			SeriesDTO dataset;
			var name = "Set-";
			int count = 0;
			var random = new Random();
			for (int i = 0; i < sectionDTO.ResistivityGeneralList.Count - 1; i++)
			{
				count++;
				dataset = new SeriesDTO();
				dataset.name = name + count.ToString();
				if ((bool)parameters.IsGraphsVisible)
					dataset.lineWidth = 0;
				dataset.lineWidth = 2;
				dataset.color = GenerateColor(i, sectionDTO.ResistivityGeneralList.Count); // String.Format("#{0:X6}", random.Next(0x1000000));
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
				for (int j = 0; j < sectionDTO.ResistivityGeneralList[i].Count; j++)
				{
					List<double> coordinates = new List<double>();

					if (i == 0 && j == 0 && !sectionDTO.ResistivityGeneralList[i][j].Checked)//First left seismic graph (İlk Sol Sismik Çizimi)
					{
						AddSeismicToGraph(sectionDTO.ResistivityGeneralList[i][j].X, sectionDTO.SeismicGeneralList[i], sectionDTO.ResistivityGeneralList[i], dataset, (byte)Enums.DirectionValue.Left, j);
					}

					#region Topography must be drawn unconditionally (Topografya (İlk Çizgi) Çizimi Koşulsuz Yapılmalı)
					if (i == 0)
					{
						if (!sectionDTO.ResistivityGeneralList[i][j].Checked)
						{
							coordinates.Add(sectionDTO.ResistivityGeneralList[i][j].X);
							coordinates.Add((double)sectionDTO.ResistivityGeneralList[i][j].K);
							dataset.data.Add(coordinates);
							sectionDTO.ResistivityGeneralList[i][j].Checked = true;

							if (j + 1 < sectionDTO.ResistivityGeneralList[i].Count)
							{
								graphDetailed = new GraphDetailedDTO { FirstNode = sectionDTO.ResistivityGeneralList[i][j].Name, SecondNode = sectionDTO.ResistivityGeneralList[i][j + 1].Name, Normal = true, Connection = "Normal" };
								graphDetailedList.Add(graphDetailed);
							}
							graphCount.Normal++;

							if (j == sectionDTO.ResistivityGeneralList[i].Count - 1)
							{
								AddSeismicToGraph(sectionDTO.ResistivityGeneralList[i][j].X, sectionDTO.SeismicGeneralList[i], sectionDTO.ResistivityGeneralList[i], dataset, (byte)Enums.DirectionValue.Right, j);
							}
						}
						continue;
					}
					#endregion

					NodeDTO suitableFirstNode = new NodeDTO();
					NodeDTO suitableSecondNode = new NodeDTO();
					if (j != sectionDTO.ResistivityGeneralList[i].Count - 1) //Last line check (Son sıra kontrolü)
					{

						suitableFirstNode = SuitableFirstNodeCheck(sectionDTO.ResistivityGeneralList, i, j);
						suitableSecondNode = SuitableSecondNodeCheck(getRule, sectionDTO, sectionDTO.ResistivityGeneralList, i, j + 1, parameters);
						if ((suitableFirstNode.Node.TypeID == (byte)Enums.ExcelDataType.Real && suitableSecondNode.Node.TypeID == (byte)Enums.ExcelDataType.Real) ||
							(suitableFirstNode.Node.TypeID == (byte)Enums.ExcelDataType.Artificial && suitableSecondNode.Node.TypeID == (byte)Enums.ExcelDataType.Real) ||
							(suitableFirstNode.Node.TypeID == (byte)Enums.ExcelDataType.Real && suitableSecondNode.Node.TypeID == (byte)Enums.ExcelDataType.Artificial))
						{
							if ((!suitableFirstNode.Node.Checked && !suitableSecondNode.Node.Checked) ||
								(suitableFirstNode.Node.Checked && !suitableSecondNode.Node.Checked) ||
								(!suitableFirstNode.Node.Checked && suitableSecondNode.Node.Checked))
							{
								if (suitableFirstNode.Node.R != null && suitableSecondNode.Node.R != null && suitableFirstNode.Node.R != 0 && suitableSecondNode.Node.R != 0)
								{

									var compareTwoNodes = _fuzzyManager.FuzzyGenerateRulesAndGetResultFLLComparison(getRule, (double)suitableFirstNode.Node.R, (double)suitableSecondNode.Node.R, (int)parameters.ResistivityRatio);

									if (compareTwoNodes) //if both resistivity values, then check speed values (iki özdirenç değeri de aynı aralıktaysa bu sefer hız değerlerine bakılır)
									{
										bool IsVpSuitable = SeismicCheckVp(sectionDTO, suitableFirstNode.IndexI, suitableFirstNode.IndexJ, (int)parameters.SeismicRatio);
										bool IsVsSuitable = SeismicCheckVs(sectionDTO, suitableFirstNode.IndexI, suitableFirstNode.IndexJ, (int)parameters.SeismicRatio);
										if (IsVpSuitable && IsVsSuitable) //If Vp, Vs and Resistivity values are suitable, then combine the nodes (Vp Vs ve Özdirenç değerleri uygunsa birleştirme yapılır)
										{
											#region Control of the seismic values where on the left of resistivity value (Özdirenç Değerinin Solunda Olan Sismik Değerlerinin Kontrolü)
											if (j == 0)
											{
												if (!sectionDTO.ResistivityGeneralList[i][j].Checked && i < sectionDTO.SeismicGeneralList.Count)
												{
													AddSeismicToGraph(sectionDTO.ResistivityGeneralList[i][j].X, sectionDTO.SeismicGeneralList[i], sectionDTO.ResistivityGeneralList[i], dataset, (byte)Enums.DirectionValue.Left, j);
												}
											}
											#endregion

											var FaultThatLeftOfFirstNode = CheckFaultOnTheLeftOfFirstNode(sectionDTO, suitableFirstNode, suitableSecondNode, datasetList);
											if (FaultThatLeftOfFirstNode != null)
											{
												List<double> coordinatesNull = new List<double>();
												dataset.data.Add(coordinatesNull);

												List<double> coordinatesLeftFault = new List<double>();
												coordinatesLeftFault.Add(FaultThatLeftOfFirstNode.data[0][0]);
												coordinatesLeftFault.Add((double)suitableFirstNode.Node.K);
												dataset.data.Add(coordinatesLeftFault);

												graphDetailed = new GraphDetailedDTO { FirstNode = "Fault (Fay)", SecondNode = suitableSecondNode.Node.Name, Normal = true, Connection = "Normal" };
												graphDetailedList.Add(graphDetailed);
												graphCount.Normal++;
											}
											if (!sectionDTO.ResistivityGeneralList[suitableFirstNode.IndexI][suitableFirstNode.IndexJ].Checked)
											{
												coordinates = new List<double>();
												coordinates.Add(suitableFirstNode.Node.X);
												coordinates.Add((double)suitableFirstNode.Node.K);
												dataset.data.Add(coordinates);
												sectionDTO.ResistivityGeneralList[suitableFirstNode.IndexI][suitableFirstNode.IndexJ].Checked = true;
											}
											var FaultThatRightOfFirstNode = CheckFaultOnTheRightOfFirstNode(sectionDTO, suitableFirstNode, suitableSecondNode, datasetList);
											if (FaultThatRightOfFirstNode != null)
											{
												List<double> coordinatesRightFault = new List<double>();
												coordinatesRightFault.Add(FaultThatRightOfFirstNode.data[0][0]);
												coordinatesRightFault.Add((double)suitableFirstNode.Node.K);
												dataset.data.Add(coordinatesRightFault);

												graphDetailed = new GraphDetailedDTO { FirstNode = suitableFirstNode.Node.Name, SecondNode = "Fay", Normal = true, Connection = "Normal" };
												graphDetailedList.Add(graphDetailed);
												graphCount.Normal++;
												continue;
											}
											coordinates = new List<double>();
											coordinates.Add(suitableSecondNode.Node.X);
											coordinates.Add((double)suitableSecondNode.Node.K);
											dataset.data.Add(coordinates);
											sectionDTO.ResistivityGeneralList[suitableSecondNode.IndexI][suitableSecondNode.IndexJ].Checked = true;

											graphDetailed = new GraphDetailedDTO { FirstNode = suitableFirstNode.Node.Name, SecondNode = suitableSecondNode.Node.Name, Normal = true, Connection = "Normal" };
											graphDetailedList.Add(graphDetailed);
											graphCount.Normal++;
										}
										else //Resistivity values are suitable but seismic values not. Check for fault and pocket  (özdirenç değerleri uygun ama sismik değerleri değil. çukur ve fay kontrolü yapılır)
										{
											if (j == 0) //Control of top level (en üst düzey kontrolü)
											{
												bool CanPocketBeDrawn = CheckPocket(datasetList, dataset, sectionDTO.ResistivityGeneralList, suitableFirstNode.IndexI, suitableSecondNode.IndexJ);

												coordinates.Add(suitableFirstNode.Node.X);
												coordinates.Add((double)suitableFirstNode.Node.K);
												dataset.data.Add(coordinates);
												sectionDTO.ResistivityGeneralList[suitableFirstNode.IndexI][suitableFirstNode.IndexJ].Checked = true;
												GeneratePocket(datasetList, dataset, sectionDTO.ResistivityGeneralList, suitableFirstNode.IndexI, suitableFirstNode.IndexJ);

												graphDetailed = new GraphDetailedDTO { FirstNode = suitableFirstNode.Node.Name, SecondNode = suitableSecondNode.Node.Name, Pocket = true, Connection = "Pocket (Kapatma)" };
												graphDetailedList.Add(graphDetailed);
												graphCount.Pocket++;
												break;
											}
											else
											{
												var previousNode = _fuzzyManager.FuzzyGenerateRulesAndGetResultFLL(getRule, (double)sectionDTO.ResistivityGeneralList[suitableFirstNode.IndexI][suitableFirstNode.IndexJ - 1].R);
												var pitComparison = _fuzzyManager.FuzzyGenerateRulesAndGetResultFLLComparison(getRule, (double)sectionDTO.ResistivityGeneralList[suitableFirstNode.IndexI][suitableFirstNode.IndexJ - 1].R, (double)suitableSecondNode.Node.R, (int)parameters.ResistivityRatio);

												bool CanPocketBeDrawn = CheckPocket(datasetList, dataset, sectionDTO.ResistivityGeneralList, suitableFirstNode.IndexI, suitableSecondNode.IndexJ);

												coordinates.Add(suitableFirstNode.Node.X);
												coordinates.Add((double)suitableFirstNode.Node.K);
												dataset.data.Add(coordinates);
												sectionDTO.ResistivityGeneralList[suitableFirstNode.IndexI][suitableFirstNode.IndexJ].Checked = true;
												GeneratePocket(datasetList, dataset, sectionDTO.ResistivityGeneralList, suitableFirstNode.IndexI, suitableSecondNode.IndexJ);

												graphDetailed = new GraphDetailedDTO { FirstNode = suitableFirstNode.Node.Name, SecondNode = suitableSecondNode.Node.Name, Pocket = true, Connection = "Kapatma" };
												graphDetailedList.Add(graphDetailed);
												graphCount.Pocket++;
												break;
											}
										}
									}
									else //resistivity values are not suitable. Check for fault and pocket (özdirenç değerleri uygun değil. fay ve kapatma kontrolü yapılır)
									{
										if (j == 0)
										{
											bool CanPocketBeDrawn = CheckPocket(datasetList, dataset, sectionDTO.ResistivityGeneralList, suitableFirstNode.IndexI, suitableSecondNode.IndexJ);

											coordinates.Add(suitableFirstNode.Node.X);
											coordinates.Add((double)suitableFirstNode.Node.K);
											dataset.data.Add(coordinates);
											sectionDTO.ResistivityGeneralList[suitableFirstNode.IndexI][suitableFirstNode.IndexJ].Checked = true;
											GeneratePocket(datasetList, dataset, sectionDTO.ResistivityGeneralList, suitableFirstNode.IndexI, suitableSecondNode.IndexJ);

											graphDetailed = new GraphDetailedDTO { FirstNode = suitableFirstNode.Node.Name, SecondNode = suitableSecondNode.Node.Name, Pocket = true, Connection = "Kapatma" };
											graphDetailedList.Add(graphDetailed);
											graphCount.Pocket++;
											break;
										}
										else
										{
											var checkForFault = CheckFault(getRule, sectionDTO, suitableFirstNode, suitableSecondNode, parameters);
											//Generate fault
											if (checkForFault && i > 1)
											{
												if (!sectionDTO.ResistivityGeneralList[suitableFirstNode.IndexI][suitableFirstNode.IndexJ].Checked)
												{
													coordinates.Add(suitableFirstNode.Node.X);
													coordinates.Add((double)suitableFirstNode.Node.K);
													dataset.data.Add(coordinates);
													sectionDTO.ResistivityGeneralList[suitableFirstNode.IndexI][suitableFirstNode.IndexJ].Checked = true;
												}

												//Fault codes (Fay oluşturma kodları)
												SeriesDTO faultDataset = GenerateFault(getRule, sectionDTO, suitableFirstNode, suitableSecondNode, parameters);
												datasetList.Add(faultDataset);

												if (!(sectionDTO.ResistivityGeneralList[suitableFirstNode.IndexI][suitableFirstNode.IndexJ].TypeID == (byte)Enums.ExcelDataType.Artificial && sectionDTO.ResistivityGeneralList[suitableFirstNode.IndexI][suitableFirstNode.IndexJ - 1].TypeID == (byte)Enums.ExcelDataType.Artificial))
												{
													List<double> fayCoordinates = new List<double>();
													fayCoordinates.Add(faultDataset.data[0][0]);
													fayCoordinates.Add((double)suitableFirstNode.Node.K);
													dataset.data.Add(fayCoordinates);
												}

												graphDetailed = new GraphDetailedDTO { FirstNode = suitableFirstNode.Node.Name, SecondNode = suitableSecondNode.Node.Name, Fault = true, Connection = "Fay" };
												graphDetailedList.Add(graphDetailed);
												graphCount.Fault++;

												continue;
											}
											else
											{

												var previousNode = _fuzzyManager.FuzzyGenerateRulesAndGetResultFLL(getRule, (double)sectionDTO.ResistivityGeneralList[suitableFirstNode.IndexI][suitableFirstNode.IndexJ - 1].R);
												var pitComparison = _fuzzyManager.FuzzyGenerateRulesAndGetResultFLLComparison(getRule, (double)sectionDTO.ResistivityGeneralList[suitableFirstNode.IndexI][suitableFirstNode.IndexJ - 1].R, (double)suitableSecondNode.Node.R, (int)parameters.ResistivityRatio);

												bool CanPocketBeDrawn = CheckPocket(datasetList, dataset, sectionDTO.ResistivityGeneralList, suitableFirstNode.IndexI, suitableSecondNode.IndexJ);
												//Generate fault
												if (CanPocketBeDrawn)
												{
													if (!sectionDTO.ResistivityGeneralList[suitableFirstNode.IndexI][suitableFirstNode.IndexJ].Checked)
													{
														coordinates.Add(suitableFirstNode.Node.X);
														coordinates.Add((double)suitableFirstNode.Node.K);
														dataset.data.Add(coordinates);
														sectionDTO.ResistivityGeneralList[suitableFirstNode.IndexI][suitableFirstNode.IndexJ].Checked = true;
													}

													GeneratePocket(datasetList, dataset, sectionDTO.ResistivityGeneralList, suitableFirstNode.IndexI, suitableSecondNode.IndexJ);

													graphDetailed = new GraphDetailedDTO { FirstNode = suitableFirstNode.Node.Name, SecondNode = suitableSecondNode.Node.Name, Pocket = true, Connection = "Kapatma" };
													graphDetailedList.Add(graphDetailed);
													graphCount.Pocket++;
													break;
												}
												var FaultThatRightOfFirstNode = CheckFaultOnTheRightOfFirstNode(sectionDTO, suitableFirstNode, suitableSecondNode, datasetList);
												if (FaultThatRightOfFirstNode != null)
												{
													List<double> coordinatesRightFault = new List<double>();
													coordinatesRightFault.Add(FaultThatRightOfFirstNode.data[0][0]);
													coordinatesRightFault.Add((double)suitableFirstNode.Node.K);
													dataset.data.Add(coordinatesRightFault);

													graphDetailed = new GraphDetailedDTO { FirstNode = suitableFirstNode.Node.Name, SecondNode = "Fay", Normal = true, Connection = "Normal" };
													graphDetailedList.Add(graphDetailed);
													graphCount.Normal++;
													continue;
												}
											}
										}
									}
								}
							}
						}
						else
						{
							sectionDTO.ResistivityGeneralList[suitableFirstNode.IndexI][suitableFirstNode.IndexJ].Checked = true;
							sectionDTO.ResistivityGeneralList[suitableSecondNode.IndexI][suitableSecondNode.IndexJ].Checked = true;
							continue;
						}
					}
					else
					{
						#region Control of the seismic values where on the right of resistivity value (Özdirenç Değerinin Sağında Olan Sismik Değerlerinin Kontrolü)
						if (sectionDTO.SeismicGeneralList.Count >= sectionDTO.ResistivityGeneralList.Count)
						{
							AddSeismicToGraph(sectionDTO.ResistivityGeneralList[i][j].X, sectionDTO.SeismicGeneralList[i], sectionDTO.ResistivityGeneralList[i], dataset, (byte)Enums.DirectionValue.Right, j);
						}

						#endregion
					}
				}
				if (dataset.data.Count > 0)
					datasetList.Add(dataset);
			}
			return datasetList;
		}

		private bool CheckPocket(List<SeriesDTO> datasetList, SeriesDTO dataset, List<List<ResistivityDTO>> resGeneralList, int indexI, int indexJ)
		{
			var topNode = resGeneralList[indexI - 1][indexJ];
			var topAndLeftNode = resGeneralList[indexI - 1][indexJ - 1];
			foreach (var item in datasetList)
			{
				for (int i = 0; i < item.data.Count; i++)
				{
					if (i + 1 < item.data.Count)
					{
						if (item.data[i].Count > 0 && item.data[i][0] == topAndLeftNode.X && item.data[i][1] == topAndLeftNode.K && item.data[i + 1][0] == topNode.X && item.data[i + 1][1] == topNode.K)
						{
							return true;
						}
					}

				}
			}

			return false;
		}

		private SeriesDTO GeneratePocket(List<SeriesDTO> datasetList, SeriesDTO dataset, List<List<ResistivityDTO>> resGeneralList, int i, int j)
		{
			if (resGeneralList.Count > i)
			{
				if (resGeneralList[i - 1].Count > j)
				{
					List<double> coordinates;

					double middlePointX = 0, middlePointK = 0;

					middlePointX = (resGeneralList[i - 1][j - 1].X + resGeneralList[i - 1][j].X) / 2;
					middlePointK = (double)(resGeneralList[i - 1][j - 1].K + resGeneralList[i - 1][j].K) / 2;

					var oncekiNoktaX = resGeneralList[i - 1][j].X;
					var oncekiNoktaK = resGeneralList[i - 1][j].K;

					coordinates = new List<double>();
					coordinates.Add(middlePointX);
					coordinates.Add((double)middlePointK);
					//datasetList.FirstOrDefault(d => d.name == oncekiDatasetName).data.Insert(index + 1, coordinates);
					dataset.data.Add(coordinates);
				}
			}
			return dataset;
		}

		private SeriesDTO CheckFaultOnTheLeftOfFirstNode(SectionDTO sectionDTO, NodeDTO suitableFirstNode, NodeDTO suitableSecondNode, List<SeriesDTO> datasetList)
		{

			if (suitableFirstNode.IndexJ > 0 && suitableFirstNode.IndexI > 0)
			{
				var fay = datasetList.Where(f => f.name == "Fault (Fay)" && f.data[0][0] < suitableFirstNode.Node.X && sectionDTO.ResistivityGeneralList[suitableFirstNode.IndexI][suitableFirstNode.IndexJ - 1].X < f.data[0][0]).FirstOrDefault();
				if (fay != null)
				{
					return fay;
				}
			}

			return null;
		}


		private SeriesDTO CheckFaultOnTheRightOfFirstNode(SectionDTO sectionDTO, NodeDTO suitableFirstNode, NodeDTO suitableSecondNode, List<SeriesDTO> datasetList)
		{
			if (suitableFirstNode.IndexJ > 0 && suitableFirstNode.IndexI > 0)
			{
				var fay = datasetList.Where(f => f.name == "Fault (Fay)" && f.data[0][0] > suitableFirstNode.Node.X && suitableSecondNode.Node.X > f.data[0][0]).FirstOrDefault();
				if (fay != null)
				{
					return fay;
				}
			}

			return null;
		}
		private bool CheckFault(GetRuleDTO getRule, SectionDTO sectionDTO, NodeDTO suitableFirstNode, NodeDTO suitableSecondNode, ParametersDTO parameters)
		{
			bool isFirstResistivityAndBottomSuitable = false, isSecondResistivityAndBottomSuitable = false;
			bool compareTwoResistivities = true, isVpSuitable = false, isVsSuitable = false, compareTwoBottomResistivites = true, isBottomVpSuitable = false, isBottomVsSuitable = false;
			int i = suitableFirstNode.IndexI;
			int j = suitableFirstNode.IndexJ;
			//Results must return false (Sonuçlar False Dönmeli)

			compareTwoResistivities = _fuzzyManager.FuzzyGenerateRulesAndGetResultFLLComparison(getRule, (double)sectionDTO.ResistivityGeneralList[suitableFirstNode.IndexI][suitableFirstNode.IndexJ].R, (double)sectionDTO.ResistivityGeneralList[suitableSecondNode.IndexI][suitableSecondNode.IndexJ].R, (int)parameters.ResistivityRatio);
			isVpSuitable = SeismicCheckVp(sectionDTO, i, j, (int)parameters.SeismicRatio);
			isVsSuitable = SeismicCheckVs(sectionDTO, i, j, (int)parameters.SeismicRatio);

			if (suitableFirstNode.IndexI + 1 + 1 < (double)sectionDTO.ResistivityGeneralList.Count && suitableSecondNode.IndexI + 1 + 1 < (double)sectionDTO.ResistivityGeneralList.Count)
			{
				//Results must return false (Sonuçlar False Dönmeli)
				compareTwoBottomResistivites = _fuzzyManager.FuzzyGenerateRulesAndGetResultFLLComparison(getRule, (double)sectionDTO.ResistivityGeneralList[suitableFirstNode.IndexI + 1][suitableFirstNode.IndexJ].R, (double)sectionDTO.ResistivityGeneralList[suitableSecondNode.IndexI + 1][suitableSecondNode.IndexJ].R, (int)parameters.ResistivityRatio);
				isBottomVpSuitable = SeismicCheckVp(sectionDTO, i + 1, j, (int)parameters.SeismicRatio);
				isBottomVsSuitable = SeismicCheckVs(sectionDTO, i + 1, j, (int)parameters.SeismicRatio);
			}

			if (suitableSecondNode.IndexI + 2 < (double)sectionDTO.ResistivityGeneralList.Count)
			{
				for (int k = i; k < (double)sectionDTO.ResistivityGeneralList[i].Count; k++)
				{
					if ((double)sectionDTO.ResistivityGeneralList[i][k].TypeID == (byte)Enums.ExcelDataType.Real)
					{
						isFirstResistivityAndBottomSuitable = _fuzzyManager.FuzzyGenerateRulesAndGetResultFLLComparison(getRule, (double)sectionDTO.ResistivityGeneralList[suitableFirstNode.IndexI][suitableFirstNode.IndexJ].R, (double)sectionDTO.ResistivityGeneralList[suitableSecondNode.IndexI + 2][suitableSecondNode.IndexJ].R, (int)parameters.ResistivityRatio);
						break;
					}
				}
			}
			if (suitableFirstNode.IndexI + 1 < (double)sectionDTO.ResistivityGeneralList.Count && suitableSecondNode.IndexI + 3 < (double)sectionDTO.ResistivityGeneralList.Count)
			{
				for (int k = i + 1; k < (double)sectionDTO.ResistivityGeneralList[i + 1].Count; k++)
				{
					if ((double)sectionDTO.ResistivityGeneralList[i + 1][k].TypeID == (byte)Enums.ExcelDataType.Real)
					{
						isSecondResistivityAndBottomSuitable = _fuzzyManager.FuzzyGenerateRulesAndGetResultFLLComparison(getRule, (double)sectionDTO.ResistivityGeneralList[suitableFirstNode.IndexI + 1][suitableFirstNode.IndexJ].R, (double)sectionDTO.ResistivityGeneralList[suitableSecondNode.IndexI + 3][suitableSecondNode.IndexJ].R, (int)parameters.ResistivityRatio);
						break;
					}
				}
			}

			if (!compareTwoResistivities &&
				isVpSuitable &&
				isVsSuitable &&
				!compareTwoBottomResistivites &&
				isBottomVpSuitable &&
				isBottomVsSuitable &&
				isFirstResistivityAndBottomSuitable &&
				isSecondResistivityAndBottomSuitable)
				return true;

			return false;
		}

		private SeriesDTO GenerateFault(GetRuleDTO getRule, SectionDTO sectionDTO, NodeDTO suitableFirstNode, NodeDTO suitableSecondNode, ParametersDTO parameters)
		{

			//assign top point (üst nokta belirle)
			var FaultStartX = (sectionDTO.ResistivityGeneralList[suitableFirstNode.IndexI - 2][suitableFirstNode.IndexJ].X + sectionDTO.ResistivityGeneralList[suitableSecondNode.IndexI - 1][suitableSecondNode.IndexJ].X) / 2;
			var FaultStartY = (sectionDTO.ResistivityGeneralList[suitableFirstNode.IndexI - 2][suitableFirstNode.IndexJ].K + sectionDTO.ResistivityGeneralList[suitableSecondNode.IndexI - 1][suitableSecondNode.IndexJ].K) / 2;

			//assign bottom point (alt nokta belirle)
			var FayBitisX = (sectionDTO.ResistivityGeneralList[suitableFirstNode.IndexI + 1][suitableFirstNode.IndexJ].X + sectionDTO.ResistivityGeneralList[suitableSecondNode.IndexI + 3][suitableSecondNode.IndexJ].X) / 2;
			var FayBitisY = sectionDTO.ResistivityGeneralList[suitableSecondNode.IndexI + 3][suitableSecondNode.IndexJ].K;
			//Draw fault (Fay ciz)
			SeriesDTO faultDataset = new SeriesDTO();
			faultDataset.name = "Fault (Fay)";
			if ((bool)parameters.IsGraphsVisible)
				faultDataset.lineWidth = 2;
			faultDataset.color = "#000000";
			faultDataset.showInLegend = false;
			faultDataset.marker = new MarkerDTO { enabled = false };
			faultDataset.tooltip = new ToolTipDTO { useHTML = true };
			faultDataset.states = new StatesDTO { hover = new HoverDTO { lineWidthPlus = 3 } };
			//fayDataset.enableMouseTracking = false;
			faultDataset.draggableY = true;
			faultDataset.draggableX = true;

			List<double> coordinates = new List<double>();
			coordinates.Add(FaultStartX);
			coordinates.Add((double)FaultStartY);
			faultDataset.data.Add(coordinates);

			coordinates = new List<double>();
			coordinates.Add(FayBitisX);
			coordinates.Add((double)FayBitisY);
			faultDataset.data.Add(coordinates);

			return faultDataset;
		}

		private string GenerateColor(int i, int count)
		{

			var color = Color.Black;
			if (i == 0) //First line is blue (ilk Çizgi Mavi)
			{
				color = Color.FromArgb(0, 0, 255);
				return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
			}
			else if (count - 1 == i) //Last linei is red (Son Çizgi Kırmızı)
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


			for (int i = 1; i < 256; i++) //From Blue to Light Blue (Maviden Açık Maviye)
			{
				RGBList.Add(new RGBDTO
				{
					R = 0,
					G = i,
					B = 255
				});
			}

			for (int i = 254; i >= 0; i--) //From Light Blue to Green (Açık Maviden Yeşile)
			{
				RGBList.Add(new RGBDTO
				{
					R = 0,
					G = 255,
					B = i
				});
			}

			for (int i = 1; i < 256; i++) //From Green to Yellow (Yeşilden Sarıya)
			{
				RGBList.Add(new RGBDTO
				{
					R = i,
					G = 255,
					B = 0
				});
			}

			for (int i = 254; i >= 0; i--) //From Yellow to Red (Sarıdan Kırmızıya)
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

		public ResultDTO GetClusterList()
		{
			ResultDTO result = new ResultDTO();
			try
			{
				var ruleList = _ruleService.Queryable().Where(k => k.AktifMi == true).Select(k => new RuleEntityDTO
				{
					RuleId = k.KuralID,
					RuleName = k.KuralAdi,
					AddDate = k.EklenmeTarihi,
					IsActive = k.AktifMi
				}).ToList();
				result.Object = ruleList;
				result.Result = true;
				result.Message = "Successful.";
				return result;
			}
			catch (Exception ex)
			{

				result.Object = null;
				result.Result = false;
				result.Message = "Unsuccessful.";
				result.Exception = ex;
				return result;
			}
		}

		public ResultDTO GetRule(long ruleId)
		{
			ResultDTO result = new ResultDTO();
			try
			{
				var ruleList = _ruleListTextService.Queryable().Where(k => k.KuralID == ruleId).Select(k => new RuleTextEntityDTO
				{
					RuleId = k.KuralID,
					RuleText = k.KuralText
				}).ToList();
				result.Object = ruleList;
				result.Result = true;
				result.Message = "Successful.";
				return result;
			}
			catch (Exception ex)
			{

				result.Object = null;
				result.Result = false;
				result.Message = "Unsuccessful.";
				result.Exception = ex;
				return result;
			}
		}

		public ResultDTO GetRuleTextAndResistivity(long ruleId)
		{
			ResultDTO result = new ResultDTO();
			try
			{
				var ruleList = _ruleListTextService.Queryable().Where(k => k.KuralID == ruleId).Select(k => new RuleTextEntityDTO
				{
					RuleId = k.KuralID,
					RuleText = k.KuralText
				}).ToList();
				var resistivityList = _variableItemService.Queryable().Where(d => d.Degisken.KuralID == ruleId && d.Degisken.DegiskenTipID == (byte)Enums.VariableType.Input).Select(d => new VariableDTO
				{
					Name = d.DegiskenItemAdi,
					MinValue = d.MinDeger,
					MaxValue = d.MaxDeger,
				}).ToList();
				result.Object = new RuleTextAndResistivityDTO { ruleTextList = ruleList, resistivityList = resistivityList };
				result.Result = true;
				result.Message = "Successful.";
				return result;
			}
			catch (Exception ex)
			{

				result.Object = null;
				result.Result = false;
				result.Message = "Unsuccessful.";
				result.Exception = ex;
				return result;
			}
		}

		private bool SeismicCheckVp(SectionDTO sectionDTO, int i, int j, int rate)
		{
			if (i < sectionDTO.SeismicGeneralList.Count && j < sectionDTO.SeismicGeneralList[i].Count)
			{
				if (sectionDTO.SeismicGeneralList[i][j].Vp != null && sectionDTO.SeismicGeneralList[i][j].Vp != 0 && sectionDTO.SeismicGeneralList[i][j].Vs != null && sectionDTO.SeismicGeneralList[i][j].Vs != 0)
				{
					if ((i + 1) < sectionDTO.SeismicGeneralList.Count)
					{
						if ((sectionDTO.SeismicGeneralList[i][j].X > sectionDTO.ResistivityGeneralList[i][j].X && sectionDTO.SeismicGeneralList[i][j].X < sectionDTO.ResistivityGeneralList[i + 1][j].X) && (sectionDTO.SeismicGeneralList[i + 1][j].X > sectionDTO.ResistivityGeneralList[i][j].X && sectionDTO.SeismicGeneralList[i + 1][j].X < sectionDTO.ResistivityGeneralList[i + 1][j].X)) //iki özdirenç arasında birden fazla sismik ölçüm olma durumu
						{
							if (sectionDTO.SeismicGeneralList[i][j].Vp > sectionDTO.SeismicGeneralList[i + 1][j].Vp)//if the Vp on the left is greater (soldaki Vp daha büyükse)
							{
								if (sectionDTO.SeismicGeneralList[i][j].Vp * (rate / 100) > sectionDTO.SeismicGeneralList[i + 1][j].Vp) //multiplication with the previous one must greater than next one (öncekinin oran ile çarpımı bir sonrakinden büyük olmalı)
								{
									return false;
								}
							}
							else //if the right one is greater (sağdaki daha büyükse)
							{
								if (sectionDTO.SeismicGeneralList[i + 1][j].Vp * (rate / 100) > sectionDTO.SeismicGeneralList[i][j].Vp)
								{
									return false;
								}
							}
						}
						else
						{
							if (j + 1 < sectionDTO.SeismicGeneralList[i].Count)
							{
								if (sectionDTO.SeismicGeneralList[i][j].Vp * (rate / 100) > sectionDTO.SeismicGeneralList[i][j + 1].Vp) //multiplication with the previous one must greater than next one (öncekinin oran ile çarpımı bir sonrakinden büyük olmalı)
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

		private bool SeismicCheckVs(SectionDTO sectionDTO, int i, int j, int rate)
		{
			if (i < sectionDTO.SeismicGeneralList.Count && j < sectionDTO.SeismicGeneralList[i].Count)
			{
				if (sectionDTO.SeismicGeneralList[i][j].Vs != null && sectionDTO.SeismicGeneralList[i][j].Vs != 0 && sectionDTO.SeismicGeneralList[i][j].Vs != null && sectionDTO.SeismicGeneralList[i][j].Vs != 0)
				{
					if ((i + 1) < sectionDTO.SeismicGeneralList.Count)
					{
						if ((sectionDTO.SeismicGeneralList[i][j].X > sectionDTO.ResistivityGeneralList[i][j].X && sectionDTO.SeismicGeneralList[i][j].X < sectionDTO.ResistivityGeneralList[i + 1][j].X) && (sectionDTO.SeismicGeneralList[i + 1][j].X > sectionDTO.ResistivityGeneralList[i][j].X && sectionDTO.SeismicGeneralList[i + 1][j].X < sectionDTO.ResistivityGeneralList[i + 1][j].X)) //iki özdirenç arasında birden fazla sismik ölçüm olma durumu
						{
							if (sectionDTO.SeismicGeneralList[i][j].Vs > sectionDTO.SeismicGeneralList[i + 1][j].Vs)//if the Vs on the left is greater (soldaki Vs daha büyükse)
							{
								if (sectionDTO.SeismicGeneralList[i][j].Vs * (rate / 100) > sectionDTO.SeismicGeneralList[i + 1][j].Vs) //multiplication with the previous one must greater than next one (öncekinin oran ile çarpımı bir sonrakinden büyük olmalı)
								{
									return false;
								}
							}
							else //if the right one is greater (sağdaki daha büyükse)
							{
								if (sectionDTO.SeismicGeneralList[i + 1][j].Vs * (rate / 100) > sectionDTO.SeismicGeneralList[i][j].Vs)
								{
									return false;
								}
							}
						}
						else
						{
							if (j + 1 < sectionDTO.SeismicGeneralList[i].Count)
							{
								if (sectionDTO.SeismicGeneralList[i][j].Vs * (rate / 100) > sectionDTO.SeismicGeneralList[i][j + 1].Vs) //multiplication with the previous one must greater than next one (öncekinin oran ile çarpımı bir sonrakinden büyük olmalı)
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

		private SeriesDTO GeneratePit(SeriesDTO dataset, List<List<ResistivityDTO>> resGeneralList, int i, int j)
		{
			SeriesDTO pitDataset = new SeriesDTO();
			pitDataset.name = "Pit (Çukur)";
			pitDataset.lineWidth = 2;
			pitDataset.color = dataset.color;
			pitDataset.showInLegend = false;
			pitDataset.marker = new MarkerDTO { enabled = false };
			pitDataset.tooltip = new ToolTipDTO { useHTML = true };
			pitDataset.states = new StatesDTO { hover = new HoverDTO { lineWidthPlus = 3 } };
			pitDataset.enableMouseTracking = false;

			var cukurBaslangicX = resGeneralList[i][j].X - (Math.Abs(resGeneralList[i][j].X - resGeneralList[i][j - 1].X) / 5);
			var cukurBaslangicK = resGeneralList[i][j].K - (Math.Abs((double)resGeneralList[i][j].K - (double)resGeneralList[i][j - 1].K) / 5);

			var cukurBitisX = resGeneralList[i][j].X + (Math.Abs(resGeneralList[i][j].X - resGeneralList[i][j + 1].X) / 5);
			var cukurBitisK = resGeneralList[i][j].K + (Math.Abs((double)resGeneralList[i][j].K - (double)resGeneralList[i][j + 1].K) / 5);

			List<double> coordinates = new List<double>();
			coordinates.Add(resGeneralList[i][j - 1].X);
			coordinates.Add((double)resGeneralList[i][j - 1].K);
			dataset.data.Add(coordinates);

			coordinates = new List<double>();
			coordinates.Add(cukurBaslangicX);
			coordinates.Add((double)cukurBaslangicK);
			dataset.data.Add(coordinates);

			coordinates = new List<double>();
			coordinates.Add(resGeneralList[i][j].X);
			coordinates.Add((double)resGeneralList[i][j].K);
			dataset.data.Add(coordinates);

			coordinates = new List<double>();
			coordinates.Add(cukurBitisX);
			coordinates.Add((double)cukurBitisK);
			dataset.data.Add(coordinates);

			coordinates = new List<double>();
			coordinates.Add(resGeneralList[i][j + 1].X);
			coordinates.Add((double)resGeneralList[i][j + 1].K);
			dataset.data.Add(coordinates);



			coordinates = new List<double>();
			coordinates.Add(cukurBaslangicX);
			coordinates.Add((double)cukurBaslangicK);
			pitDataset.data.Add(coordinates);

			coordinates = new List<double>();
			coordinates.Add(resGeneralList[i][j].X + 1);
			coordinates.Add((double)resGeneralList[i][j].K);
			pitDataset.data.Add(coordinates);

			coordinates = new List<double>();
			coordinates.Add(cukurBitisX);
			coordinates.Add((double)cukurBitisK);
			pitDataset.data.Add(coordinates);

			return pitDataset;
		}

		/// <summary>
		/// Connect the seismic values to graph where the values are left or right of the resistivity value (Özdirenç Değerinin Sağında Veya Solunda Bulunan Sismik Değerlerini Çizime Bağlar)
		/// </summary>
		private void AddSeismicToGraph(double ResistivityX, List<SeismicDTO> seismicList, List<ResistivityDTO> resistivityList, SeriesDTO dataset, byte Direction, int j)
		{

			var SeismicList = seismicList.Where(s => Direction == (byte)Enums.DirectionValue.Left ? s.X < ResistivityX : s.X > ResistivityX).ToList();
			for (int i = 0; i < SeismicList.Count; i++)
			{
				GraphDetailedDTO graphDetailed = new GraphDetailedDTO();

				List<double> coordinates = new List<double>();
				coordinates.Add(SeismicList[i].X);
				coordinates.Add((double)SeismicList[i].K);
				dataset.data.Add(coordinates);


				if (Direction == (byte)Enums.DirectionValue.Left)//Çizimin solundaki sismik değerleri kontrol ediliyorsa
				{
					if (SeismicList.Count > 1)
					{
						if (i < SeismicList.Count - 1)
						{
							graphDetailed = new GraphDetailedDTO { FirstNode = SeismicList[i].Name, SecondNode = SeismicList[i + 1].Name, Normal = true, Connection = "Normal" };
							graphDetailedList.Add(graphDetailed);
						}
						else
						{
							graphDetailed = new GraphDetailedDTO { FirstNode = SeismicList[i].Name, SecondNode = resistivityList[j].Name, Normal = true, Connection = "Normal" };
							graphDetailedList.Add(graphDetailed);
						}
					}
					else
					{
						graphDetailed = new GraphDetailedDTO { FirstNode = SeismicList[i].Name, SecondNode = resistivityList[j].Name, Normal = true, Connection = "Normal" };
						graphDetailedList.Add(graphDetailed);
					}
				}
				else//Çizimin sağındaki sismik değerleri kontrol ediliyorsa
				{
					if (SeismicList.Count > 1)
					{
						if (i < SeismicList.Count - 1)
						{
							graphDetailed = new GraphDetailedDTO { FirstNode = SeismicList[i].Name, SecondNode = SeismicList[i + 1].Name, Normal = true, Connection = "Normal" };
							graphDetailedList.Add(graphDetailed);
						}
						else
						{
							graphDetailed = new GraphDetailedDTO { FirstNode = resistivityList[j].Name, SecondNode = SeismicList[i].Name, Normal = true, Connection = "Normal" };
							graphDetailedList.Add(graphDetailed);
						}
					}
					else
					{
						graphDetailed = new GraphDetailedDTO { FirstNode = resistivityList[j].Name, SecondNode = SeismicList[i].Name, Normal = true, Connection = "Normal" };
						graphDetailedList.Add(graphDetailed);
					}
				}



				graphCount.Normal++;
			}
		}


		private NodeDTO SuitableFirstNodeCheck(List<List<ResistivityDTO>> resGeneralList, int indexI, int indexJ)
		{
			NodeDTO node = new NodeDTO { Node = resGeneralList[indexI][indexJ], IndexI = indexI, IndexJ = indexJ };

			for (int i = indexI - 1; i >= 0; i--)
			{
				if (!resGeneralList[i][indexJ].Checked && resGeneralList[i][indexJ].TypeID == (byte)Enums.ExcelDataType.Real)
				{
					if (!resGeneralList[i - 1][indexJ].Checked)
					{
						return node;
					}
					node.IndexI = i;
					node.IndexJ = indexJ;
					node.Node = resGeneralList[i][indexJ];
					break;
				}
			}


			return node;
		}

		private NodeDTO SuitableSecondNodeCheck(GetRuleDTO getRule, SectionDTO sectionDTO, List<List<ResistivityDTO>> resGeneralList, int indexI, int indexJ, ParametersDTO parameters)
		{
			NodeDTO node = new NodeDTO { Node = resGeneralList[indexI][indexJ], IndexI = indexI, IndexJ = indexJ };

			for (int i = indexI - 1; i >= 0; i--)
			{
				if (!resGeneralList[i][indexJ].Checked && resGeneralList[i][indexJ].TypeID == (byte)Enums.ExcelDataType.Real)
				{

					node.IndexI = i;
					node.IndexJ = indexJ;
					node.Node = resGeneralList[i][indexJ];
				}
			}

			return node;
		}

		#region Old Separate Control Codes (Eski Ayrı Ayrı Kontrol Kodları)
		private List<SeriesDTO> GenerateGraphData(List<List<ResistivityDTO>> resGeneralList)
		{
			List<SeriesDTO> datasetList = new List<SeriesDTO>();
			SeriesDTO dataset;
			var name = "Set-";
			int count = 0;
			var random = new Random();
			foreach (var rezList in resGeneralList)
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
		private List<SeriesDTO> GenerateGraphData(List<List<SeismicDTO>> seisGeneralList)
		{
			List<SeriesDTO> datasetList = new List<SeriesDTO>();
			SeriesDTO dataset;
			var name = "Set-";
			int count = 0;
			var random = new Random();
			foreach (var sisList in seisGeneralList)
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
		private List<SeriesDTO> GenerateGraphData(List<List<DrillDTO>> driGeneralList)
		{
			List<SeriesDTO> datasetList = new List<SeriesDTO>();
			SeriesDTO dataset;
			var name = "Set-";
			int count = 0;
			var random = new Random();
			foreach (var rezList in driGeneralList)
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
		ResultDTO CheckExcel(ExcelModelDTO excel, string path);
		ResultDTO GenerateGraph(GraphDTO graph, string path);
		List<SeriesDTO> GenerateGraphData(long ruleId, SectionDTO sectionDTO, ParametersDTO parameters);
		ResultDTO GetClusterList();
		ResultDTO GetRule(long ruleId);
		ResultDTO GetRuleTextAndResistivity(long ruleId);
	}
}
