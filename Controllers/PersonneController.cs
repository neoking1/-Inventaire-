using Microsoft.AspNetCore.Mvc;
using Inventaire.Data;

namespace Inventaire.Controllers
{
    public class PersonneController : BaseController
    {
        private readonly DbHelper _db;
        public PersonneController(DbHelper db) { _db = db; }

        public IActionResult Index()
        {
            var check = CheckAccess(CanViewPersonnes());
            if (check != null) return check;
            SetUserViewBag();
            return View();
        }
    }
}
