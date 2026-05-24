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

            var data = _db.ExecuteQuery(@"
                SELECT u.id, u.login, u.type, u.personne_id,
                       p.nom, p.prenom, p.n_tel
                FROM users u
                LEFT JOIN personne p ON u.personne_id = p.personne_id
                ORDER BY u.id");

            ViewBag.Total = data.Rows.Count;
            ViewBag.Personnes = _db.ExecuteQuery("SELECT personne_id, nom, prenom FROM personne ORDER BY nom");
            return View(data);
        }

        [HttpPost]
        public IActionResult Create(string login, string mdp, string type, int? personneId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(mdp))
                    return Json(new { success = false, message = "Login et mot de passe requis." });

                _db.ExecuteNonQuery(
                    "INSERT INTO users (login, mdp, type, personne_id) VALUES (@login, @mdp, @type, @pid)",
                    new Dictionary<string, object> {
                        {"@login", login.Trim()}, {"@mdp", mdp}, {"@type", type ?? "viewer"},
                        {"@pid", (object?)personneId ?? DBNull.Value}
                    });
                return Json(new { success = true, message = "Utilisateur créé." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public IActionResult Update(int id, string login, string mdp, string type, int? personneId)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(mdp))
                {
                    _db.ExecuteNonQuery(
                        "UPDATE users SET login=@login, mdp=@mdp, type=@type, personne_id=@pid WHERE id=@id",
                        new Dictionary<string, object> {
                            {"@login", login.Trim()}, {"@mdp", mdp}, {"@type", type ?? "viewer"},
                            {"@pid", (object?)personneId ?? DBNull.Value}, {"@id", id}
                        });
                }
                else
                {
                    _db.ExecuteNonQuery(
                        "UPDATE users SET login=@login, type=@type, personne_id=@pid WHERE id=@id",
                        new Dictionary<string, object> {
                            {"@login", login.Trim()}, {"@type", type ?? "viewer"},
                            {"@pid", (object?)personneId ?? DBNull.Value}, {"@id", id}
                        });
                }
                return Json(new { success = true, message = "Utilisateur mis à jour." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                var currentId = HttpContext.Session.GetInt32("UserId");
                if (currentId == id)
                    return Json(new { success = false, message = "Vous ne pouvez pas supprimer votre propre compte." });

                _db.ExecuteNonQuery("DELETE FROM users WHERE id=@id",
                    new Dictionary<string, object> { { "@id", id } });
                return Json(new { success = true, message = "Utilisateur supprimé." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }
    }
}

