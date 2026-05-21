using Microsoft.AspNetCore.Mvc;

namespace Inventaire.Controllers
{
    public class ReportsController : BaseController
    {
        public IActionResult Index()
        {
            var check = CheckAccess(CanViewReports());
            if (check != null) return check;
            SetUserViewBag();
            return View();
        }
    }
}
