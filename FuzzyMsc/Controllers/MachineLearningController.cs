using FuzzyMsc.Bll;
using FuzzyMsc.Dto;
using FuzzyMsc.Dto.CizimDTOS;
using System.Web.Mvc;

namespace FuzzyMsc.Controllers
{
	public class MachineLearningController : Controller
    {
		IMachineLearningManager _machineLearningManager;

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

		public JsonResult Test(MachineLearningDTO datas) {
			SonucDTO sonuc = _machineLearningManager.Test(datas);

			return Json(new { Sonuc = sonuc.Sonuc, Mesaj = sonuc.Mesaj, Nesne = sonuc.Nesne, Exception = sonuc.Exception.ToString() }, JsonRequestBehavior.AllowGet);
		}
	}
}