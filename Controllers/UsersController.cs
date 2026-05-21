using Microsoft.AspNetCore.Mvc;
using Inventaire.Data;

namespace Inventaire.Controllers
{
    public class UsersController : BaseController
    {
        private readonly DbHelper _db;
        public UsersController(DbHelper db) { _db = db; }

        public IActionResult Index()
        {
            var check = CheckAccess(CanViewUsers());
            if (check != null) return check;
            SetUserViewBag();
            return View();
        }
    }
}
