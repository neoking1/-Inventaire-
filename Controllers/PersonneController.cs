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
            var data = _db.ExecuteQuery("SELECT personne_id, nom, prenom, n_tel FROM personne ORDER BY personne_id");
            ViewBag.Total = data.Rows.Count;
            return View(data);
        }

        [HttpPost]
        public IActionResult Create(string nom, string prenom, string nTel)
        {
            try
            {
                _db.ExecuteNonQuery("INSERT INTO personne (nom, prenom, n_tel) VALUES (@nom,@prenom,@ntel)",
                    new Dictionary<string, object> { { "@nom", nom ?? "" }, { "@prenom", prenom ?? "" }, { "@ntel", nTel ?? "" } });
                return Json(new { success = true, message = "Personne ajoutée." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public IActionResult Update(int id, string nom, string prenom, string nTel)
        {
            try
            {
                _db.ExecuteNonQuery("UPDATE personne SET nom=@nom, prenom=@prenom, n_tel=@ntel WHERE personne_id=@id",
                    new Dictionary<string, object> { { "@nom", nom ?? "" }, { "@prenom", prenom ?? "" }, { "@ntel", nTel ?? "" }, { "@id", id } });
                return Json(new { success = true, message = "Personne mise à jour." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                _db.ExecuteNonQuery("DELETE FROM personne WHERE personne_id=@id",
                    new Dictionary<string, object> { { "@id", id } });
                return Json(new { success = true, message = "Personne supprimée." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }
    }
}

