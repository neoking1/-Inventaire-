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

            var data = _db.ExecuteQuery(
                "SELECT fournisseur_id, raison_social, contact, fax, email, adress, ville, remarque FROM fournisseurs ORDER BY fournisseur_id");

            ViewBag.Total = data.Rows.Count;
            return View(data);
        }

        [HttpPost]
        public IActionResult Create(string raisonSocial, string contact, string fax, string email, string adress, string ville, string remarque)
        {
            try
            {
                _db.ExecuteNonQuery(
                    "INSERT INTO fournisseurs (raison_social,contact,fax,email,adress,ville,remarque) VALUES (@rs,@co,@fax,@em,@ad,@vi,@rem)",
                    new Dictionary<string, object> {
                        {"@rs", raisonSocial??""}, {"@co", contact??""}, {"@fax", fax??""},
                        {"@em", email??""}, {"@ad", adress??""}, {"@vi", ville??""}, {"@rem", remarque??""}
                    });
                return Json(new { success = true, message = "Fournisseur ajouté." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public IActionResult Update(int id, string raisonSocial, string contact, string fax, string email, string adress, string ville, string remarque)
        {
            try
            {
                _db.ExecuteNonQuery(
                    "UPDATE fournisseurs SET raison_social=@rs,contact=@co,fax=@fax,email=@em,adress=@ad,ville=@vi,remarque=@rem WHERE fournisseur_id=@id",
                    new Dictionary<string, object> {
                        {"@rs", raisonSocial??""}, {"@co", contact??""}, {"@fax", fax??""},
                        {"@em", email??""}, {"@ad", adress??""}, {"@vi", ville??""}, {"@rem", remarque??""}, {"@id", id}
                    });
                return Json(new { success = true, message = "Fournisseur mis à jour." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                _db.ExecuteNonQuery("DELETE FROM fournisseurs WHERE fournisseur_id=@id",
                    new Dictionary<string, object> { { "@id", id } });
                return Json(new { success = true, message = "Fournisseur supprimé." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }
    }
}

