using Microsoft.AspNetCore.Mvc;
using Inventaire.Data;

namespace Inventaire.Controllers
{
    public class OriginController : BaseController
    {
        private readonly DbHelper _db;
        public OriginController(DbHelper db) { _db = db; }

        public IActionResult Index()
        {
            var check = CheckAccess(CanViewFournisseurs());
            if (check != null) return check;
            SetUserViewBag();
            return View();
        }
    }
}
