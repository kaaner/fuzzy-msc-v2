using FuzzyMsc.Bll;
using FuzzyMsc.Dto;
using FuzzyMsc.Dto.CizimDTOS;
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
		IOrtakManager _ortakManager;

		string uploadTmpFolder = "~/App_Data/Tmp";
		string uploadFolder = "~/App_Data/Tmp/FileUploads";

		public GraphController(IGraphManager graphManager,
			IOrtakManager ortakManager)
		{
			_graphManager = graphManager;
			_ortakManager = ortakManager;
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
			var sonuc = _graphManager.KumeListesiGetir();
			return Json(new { Sonuc = sonuc.Sonuc, Mesaj = sonuc.Mesaj, Nesne = sonuc.Nesne, Exception = sonuc.Exception }, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public JsonResult KuralGetir(long kuralID)
		{
			var sonuc = _graphManager.KuralGetir(kuralID);
			return Json(new { Sonuc = sonuc.Sonuc, Mesaj = sonuc.Mesaj, Nesne = sonuc.Nesne, Exception = sonuc.Exception }, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public JsonResult KuralTextVeOzdirencGetir(long kuralID)
		{
			var sonuc = _graphManager.KuralTextVeOzdirencGetir(kuralID);
			return Json(new { Sonuc = sonuc.Sonuc, Mesaj = sonuc.Mesaj, Nesne = sonuc.Nesne, Exception = sonuc.Exception }, JsonRequestBehavior.AllowGet);
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

			return Json(new { Sonuc = true, Mesaj = "Basarili", Nesne = new ExcelModelDTO { adi = fileName, data = Convert.ToBase64String(entityItem), path = path } }, JsonRequestBehavior.AllowGet);
			//return entityItem;
		}

		[AllowAnonymous]
		[HttpPost]
		public JsonResult ExcelKontrolEt(ExcelModelDTO excel)
		{
			var path = Path.Combine(Server.MapPath(uploadFolder), excel.adi);
			var sonuc = _graphManager.ExcelKontrolEt(excel, path);
			return Json(new { Sonuc = sonuc.Sonuc, Mesaj = sonuc.Mesaj, Nesne = sonuc.Nesne, Exception = sonuc.Exception }, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult GraphOlustur(GraphDTO graph)
		{
			SonucDTO sonuc = new SonucDTO();
			try
			{
				var path = Path.Combine(Server.MapPath(uploadFolder), graph.excel.adi);
				sonuc = _graphManager.GraphOlustur(graph, path);
				return Json(new { Sonuc = sonuc.Sonuc, Mesaj = sonuc.Mesaj, Nesne = sonuc.Nesne, Exception = sonuc.Exception.ToString() }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				return Json(new { Sonuc = sonuc.Sonuc, Mesaj = ex.Message, Nesne = sonuc.Nesne, Exception = ex.ToString() }, JsonRequestBehavior.AllowGet);
			}
		}
	}
}