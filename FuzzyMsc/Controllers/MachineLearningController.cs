using FuzzyMsc.Bll;
using FuzzyMsc.Dto;
using FuzzyMsc.Dto.MachineLearningDTOS;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace FuzzyMsc.Controllers
{
	public class MachineLearningController : Controller
	{
		IMachineLearningManager _machineLearningManager;
		string uploadTmpFolder = "~/App_Data/Tmp";

		public MachineLearningController(IMachineLearningManager machineLearningManager)
		{
			_machineLearningManager = machineLearningManager;
		}
		// GET: MachineLearning
		public ActionResult Index()
		{
			//_machineLearningManager.Test();
			return View();
		}

		[HttpPost]
		public JsonResult Test(MachineLearningDTO datas)
		{
			SonucDTO sonuc = _machineLearningManager.Test(datas);

			return Json(new { Sonuc = sonuc.Sonuc, Mesaj = sonuc.Mesaj, Nesne = sonuc.Nesne }, JsonRequestBehavior.AllowGet);
		}
		[HttpPost]

		public JsonResult CreateAndSaveModel()
		{
			SonucDTO sonuc = _machineLearningManager.CreateAndSaveModel();

			return Json(new { Sonuc = sonuc.Sonuc, Mesaj = sonuc.Mesaj, Nesne = sonuc.Nesne }, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult GetFullPath()
		{
			var file = Request.Files[0];
			var fileName = Path.GetFileName(file.FileName);

			var path = Path.Combine(Server.MapPath(uploadTmpFolder), fileName);

			return Json(new { Sonuc = true, Mesaj = "Basarili", Nesne = path }, JsonRequestBehavior.AllowGet);
		}
	}
}