using FuzzyMsc.Bll;
using FuzzyMsc.Dto;
using FuzzyMsc.Dto.FuzzyDTOS;
using System.Collections.Generic;
using System.Web.Mvc;

namespace FuzzyMsc.Controllers
{
	public class FuzzyController : Controller
	{
		IUserManager _userManager;
		IFuzzyManager _fuzzyManager;
		public FuzzyController(IUserManager userManager,
			IFuzzyManager fuzzyManager)
		{
			_userManager = userManager;
			_fuzzyManager = fuzzyManager;
		}
		// GET: Fuzzy
		public ActionResult Index()
		{
			return View();
		}

		[HttpGet]
		public JsonResult Test()
		{
			_fuzzyManager.Test(12, 12, 12);
			ResultDTO sonuc = _userManager.Get();
			return Json(new { Sonuc = sonuc.Result, Mesaj = sonuc.Message, Nesne = sonuc.Object, Exception = sonuc.Exception }, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult Sonuclar(List<FuzzyDTO> zeminList)
		{
			List<FuzzyResultDTO> donenDegerList = new List<FuzzyResultDTO>();
			try
			{
				foreach (var item in zeminList)
				{
					var donenDeger = _fuzzyManager.Test(item.Resistivity, item.Resistance, item.Saturation);
					donenDegerList.Add(new FuzzyResultDTO { Result = donenDeger });
				}
				return Json(new { Sonuc = true, Mesaj = "İşlem Başarılı", Nesne = donenDegerList, }, JsonRequestBehavior.AllowGet);
			}
			catch (System.Exception ex)
			{
				return Json(new { Mesaj = "Başarısız", Exception = ex }, JsonRequestBehavior.AllowGet);
			}
		}

		public ActionResult Kume()
		{
			return View();
		}

		[HttpPost]
		public JsonResult KumeKaydet(RuleClusterDTO kuralKume)
		{
			var sonuc = _fuzzyManager.SaveCluster(kuralKume);

			return Json(new { Sonuc = sonuc.Result, Nesne = sonuc.Object, Mesaj = sonuc.Message, Exception = sonuc.Exception }, JsonRequestBehavior.AllowGet);

		}
	}
}