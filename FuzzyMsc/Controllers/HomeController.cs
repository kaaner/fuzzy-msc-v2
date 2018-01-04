using FuzzyMsc.Bll;
using FuzzyMsc.Dto;
using System.Web.Mvc;

namespace FuzzyMsc.Controllers
{
    public class HomeController : Controller
    {
        IKullaniciManager _kullaniciManager;
        IFuzzyManager _fuzzyManager;
        public HomeController(IKullaniciManager kullaniciManager,
            IFuzzyManager fuzzyManager)
        {
            _kullaniciManager = kullaniciManager;
            _fuzzyManager = fuzzyManager;
        }
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult Kaydet() {

            _fuzzyManager.Test(12,12,12);
            SonucDTO sonuc = _kullaniciManager.Getir();
            return Json(new { Sonuc = sonuc.Sonuc, Mesaj = sonuc.Mesaj, Nesne = sonuc.Nesne, Exception = sonuc.Exception }, JsonRequestBehavior.AllowGet);
        }
    }
}