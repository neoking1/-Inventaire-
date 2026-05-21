using Microsoft.AspNetCore.Mvc;
using Inventaire.Data;

namespace Inventaire.Controllers
{
    public class SortiesController : BaseController
    {
        private readonly DbHelper _db;
        public SortiesController(DbHelper db) { _db = db; }

        public IActionResult Index()
        {
            var check = CheckAccess(IsViewer());
            if (check != null) return check;
            SetUserViewBag();
            return View();
        }
    }
}