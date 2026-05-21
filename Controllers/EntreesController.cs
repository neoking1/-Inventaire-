using Microsoft.AspNetCore.Mvc;
using Inventaire.Data;

namespace Inventaire.Controllers
{
    public class EntreesController : BaseController
    {
        private readonly DbHelper _db;
        public EntreesController(DbHelper db) { _db = db; }

        public IActionResult Index()
        {
            var check = CheckAccess(IsViewer());
            if (check != null) return check;
            SetUserViewBag();
            return View();
        }
    }
}