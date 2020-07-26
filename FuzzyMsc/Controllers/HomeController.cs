using FuzzyMsc.Bll;
using FuzzyMsc.Dto;
using System.Web.Mvc;

namespace FuzzyMsc.Controllers
{
    public class HomeController : Controller
    {
        IUserManager _userManager;
        IFuzzyManager _fuzzyManager;
        public HomeController(IUserManager userManager,
            IFuzzyManager fuzzyManager)
        {
            _userManager = userManager;
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
            ResultDTO sonuc = _userManager.Get();
            return Json(new { Sonuc = sonuc.Result, Mesaj = sonuc.Message, Nesne = sonuc.Object, Exception = sonuc.Exception }, JsonRequestBehavior.AllowGet);
        }
    }
}