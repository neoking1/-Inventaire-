using Microsoft.AspNetCore.Mvc;
using Inventaire.Data;

namespace Inventaire.Controllers
{
    public class FournisseursController : BaseController
    {
        private readonly DbHelper _db;
        public FournisseursController(DbHelper db) { _db = db; }

        public IActionResult Index()
        {
            var check = CheckAccess(CanViewFournisseurs());
            if (check != null) return check;
            SetUserViewBag();
            return View();
        }
    }
}
