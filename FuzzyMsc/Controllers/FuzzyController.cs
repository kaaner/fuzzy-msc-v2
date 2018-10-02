using FuzzyMsc.Bll;
using FuzzyMsc.Dto;
using FuzzyMsc.Dto.FuzzyDTOS;
using System.Collections.Generic;
using System.Web.Mvc;

namespace FuzzyMsc.Controllers
{
	public class FuzzyController : Controller
	{
		IKullaniciManager _kullaniciManager;
		IFuzzyManager _fuzzyManager;
		public FuzzyController(IKullaniciManager kullaniciManager,
			IFuzzyManager fuzzyManager)
		{
			_kullaniciManager = kullaniciManager;
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
			SonucDTO sonuc = _kullaniciManager.Getir();
			return Json(new { Sonuc = sonuc.Sonuc, Mesaj = sonuc.Mesaj, Nesne = sonuc.Nesne, Exception = sonuc.Exception }, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult Sonuclar(List<FuzzyDTO> zeminList)
		{
			List<FuzzyResultDTO> donenDegerList = new List<FuzzyResultDTO>();
			try
			{
				foreach (var item in zeminList)
				{
					var donenDeger = _fuzzyManager.Test(item.Ozdirenc, item.Mukavemet, item.Doygunluk);
					donenDegerList.Add(new FuzzyResultDTO { Sonuc = donenDeger });
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
		public JsonResult KumeKaydet(KuralKumeDTO kuralKume)
		{
			var sonuc = _fuzzyManager.KumeKaydet(kuralKume);

			return Json(new { Sonuc = sonuc.Sonuc, Nesne = sonuc.Nesne, Mesaj = sonuc.Mesaj, Exception = sonuc.Exception }, JsonRequestBehavior.AllowGet);

		}
	}
}