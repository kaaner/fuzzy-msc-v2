using FuzzyMsc.Bll;
using FuzzyMsc.Dto;
using FuzzyMsc.Dto.GraphDTOS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FuzzyMsc.Controllers
{
	public class GraphController : Controller
	{
		IGraphManager _graphManager;
		ICommonManager _commonManager;

		string uploadTmpFolder = "~/App_Data/Tmp";
		string uploadFolder = "~/App_Data/Tmp/FileUploads";

		public GraphController(IGraphManager graphManager,
			ICommonManager commonManager)
		{
			_graphManager = graphManager;
			_commonManager = commonManager;
		}
		// GET: Graph
		public ActionResult Index()
		{
			return View();
		}

		public ActionResult Cizim()
		{
			return View();
		}
		[HttpGet]
		public JsonResult KumeListesiGetir()
		{
			var sonuc = _graphManager.GetClusterList();
			return Json(new { Sonuc = sonuc.Result, Mesaj = sonuc.Message, Nesne = sonuc.Object, Exception = sonuc.Exception }, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public JsonResult KuralGetir(long kuralID)
		{
			var sonuc = _graphManager.GetRule(kuralID);
			return Json(new { Sonuc = sonuc.Result, Mesaj = sonuc.Message, Nesne = sonuc.Object, Exception = sonuc.Exception }, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public JsonResult KuralTextVeOzdirencGetir(long kuralID)
		{
			var sonuc = _graphManager.GetRuleTextAndResistivity(kuralID);
			return Json(new { Sonuc = sonuc.Result, Mesaj = sonuc.Message, Nesne = sonuc.Object, Exception = sonuc.Exception }, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult UploadExcel()
		{
			var file = Request.Files[0];
			var fileName = Path.GetFileName(file.FileName);

			var path = Path.Combine(Server.MapPath(uploadTmpFolder), fileName);
			file.SaveAs(path);
			var entityItem = new byte[file.ContentLength];
			file.InputStream.Read(entityItem, 0, file.ContentLength);
			file.InputStream.Close();
			DirectoryInfo di = new DirectoryInfo(Server.MapPath(uploadTmpFolder));
			//FileInfo[] rgFiles = di.GetFiles();
			//foreach (var item in rgFiles)
			//{
			//	System.IO.File.Delete(item.FullName);
			//}

			return Json(new { Sonuc = true, Mesaj = "Basarili", Nesne = new ExcelModelDTO { name = fileName, data = Convert.ToBase64String(entityItem), path = path } }, JsonRequestBehavior.AllowGet);
			//return entityItem;
		}

		[AllowAnonymous]
		[HttpPost]
		public JsonResult ExcelKontrolEt(ExcelModelDTO excel)
		{
			var path = Path.Combine(Server.MapPath(uploadFolder), excel.name);
			var sonuc = _graphManager.CheckExcel(excel, path);
			return Json(new { Sonuc = sonuc.Result, Mesaj = sonuc.Message, Nesne = sonuc.Object, Exception = sonuc.Exception }, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult GraphOlustur(GraphDTO graph)
		{
			ResultDTO sonuc = new ResultDTO();
			try
			{
				var path = Path.Combine(Server.MapPath(uploadFolder), graph.excel.name);
				sonuc = _graphManager.GenerateGraph(graph, path);
				return Json(new { Sonuc = sonuc.Result, Mesaj = sonuc.Message, Nesne = sonuc.Object, Exception = sonuc.Exception.ToString() }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				return Json(new { Sonuc = sonuc.Result, Mesaj = ex.Message, Nesne = sonuc.Object, Exception = ex.ToString() }, JsonRequestBehavior.AllowGet);
			}
		}
	}
}