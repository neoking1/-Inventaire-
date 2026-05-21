using Microsoft.AspNetCore.Mvc;
using Inventaire.Data;

namespace Inventaire.Controllers
{
    public class LocaliteController : BaseController
    {
        private readonly DbHelper _db;
        public LocaliteController(DbHelper db) { _db = db; }

        public IActionResult Index()
        {
            var check = CheckAccess(IsViewer());
            if (check != null) return check;
            SetUserViewBag();
            return View();
        }
    }
}