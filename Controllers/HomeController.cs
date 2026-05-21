using Microsoft.AspNetCore.Mvc;

namespace Inventaire.Controllers
{
    public class HomeController : BaseController
    {
        public IActionResult Index()
        {
            var check = CheckAccess(IsViewer());
            if (check != null) return check;
            SetUserViewBag();
            return View();
        }
    }
}