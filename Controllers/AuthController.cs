using Microsoft.AspNetCore.Mvc;
using Inventaire.Data;
using System.Data;
using System.Linq;

namespace Inventaire.Controllers
{
    public class AuthController : Controller
    {
        private readonly DbHelper _db;
        public AuthController(DbHelper db) { _db = db; }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string login, string mdp)
        {
            var result = _db.ExecuteQuery(
                "SELECT * FROM users WHERE login = @login AND mdp = @mdp",
                new Dictionary<string, object> { { "@login", login }, { "@mdp", mdp } }
            );

            if (result.Rows.Count > 0)
            {
                HttpContext.Session.SetInt32("UserId", Convert.ToInt32(result.Rows[0]["id"]));
                HttpContext.Session.SetString("UserLogin", result.Rows[0]["login"].ToString()!);
                HttpContext.Session.SetString("UserType", result.Rows[0]["type"].ToString()!);
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Login ou mot de passe incorrect.";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(
            string nom,
            string prenom,
            string n_tel,
            string cin,
            string email,
            string localite,
            string login,
            string mdp,
            string confirm_mdp)
        {
            ViewBag.Nom = nom;
            ViewBag.Prenom = prenom;
            ViewBag.NTel = n_tel;
            ViewBag.Cin = cin;
            ViewBag.Email = email;
            ViewBag.Localite = localite;
            ViewBag.Login = login;
            int? createdPersonneId = null;

            try
            {
                if (string.IsNullOrWhiteSpace(nom) ||
                    string.IsNullOrWhiteSpace(prenom) ||
                    string.IsNullOrWhiteSpace(n_tel) ||
                    string.IsNullOrWhiteSpace(cin) ||
                    string.IsNullOrWhiteSpace(login) ||
                    string.IsNullOrWhiteSpace(mdp))
                {
                    ViewBag.Error = "Tous les champs requis doivent \u00EAtre remplis.";
                    return View();
                }

                if (!string.Equals(mdp, confirm_mdp, StringComparison.Ordinal))
                {
                    ViewBag.Error = "La confirmation du mot de passe ne correspond pas.";
                    return View();
                }

                var loginExists = Convert.ToInt32(_db.ExecuteScalar(
                    "SELECT COUNT(1) FROM users WHERE login=@login",
                    new Dictionary<string, object> { { "@login", login.Trim() } }) ?? 0) > 0;
                if (loginExists)
                {
                    ViewBag.Error = "Ce login existe d\u00E9j\u00E0.";
                    return View();
                }

                var localiteInput = localite?.Trim() ?? string.Empty;
                int? localiteId = ResolveLocaliteId(localiteInput);
                if (!string.IsNullOrWhiteSpace(localiteInput) && localiteId == null)
                {
                    ViewBag.Error = "Localit\u00E9 introuvable.";
                    return View();
                }

                var hasCinColumn = HasPersonneColumn("cin");
                var hasLocaliteIdColumn = HasPersonneColumn("localite_id");

                var personneColumns = new List<string> { "nom", "prenom", "n_tel" };
                var personneParams = new Dictionary<string, object>
                {
                    { "@nom", nom.Trim() },
                    { "@prenom", prenom.Trim() },
                    { "@n_tel", n_tel.Trim() }
                };

                if (hasCinColumn)
                {
                    personneColumns.Add("cin");
                    personneParams["@cin"] = cin.Trim();
                }

                if (hasLocaliteIdColumn)
                {
                    personneColumns.Add("localite_id");
                    personneParams["@localite_id"] = (object?)localiteId ?? DBNull.Value;
                }

                var personneValues = personneColumns.Select(c => "@" + c);
                var personneInsertSql =
                    $"INSERT INTO personne ({string.Join(", ", personneColumns)}) VALUES ({string.Join(", ", personneValues)}); " +
                    "SELECT CAST(SCOPE_IDENTITY() AS int);";

                var personneIdRaw = _db.ExecuteScalar(personneInsertSql, personneParams);
                if (personneIdRaw == null || personneIdRaw == DBNull.Value)
                {
                    ViewBag.Error = "Impossible de cr\u00E9er la personne.";
                    return View();
                }

                createdPersonneId = Convert.ToInt32(personneIdRaw);
                _db.ExecuteNonQuery(
                    "INSERT INTO users (login, mdp, type, personne_id) VALUES (@login, @mdp, @type, @pid)",
                    new Dictionary<string, object>
                    {
                        { "@login", login.Trim() },
                        { "@mdp", mdp },
                        { "@type", "viewer" },
                        { "@pid", createdPersonneId.Value }
                    });

                TempData["Success"] = "Compte cr\u00E9\u00E9 avec succ\u00E8s. Connectez-vous.";
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                if (createdPersonneId.HasValue)
                {
                    try
                    {
                        _db.ExecuteNonQuery(
                            "DELETE FROM personne WHERE personne_id=@id AND NOT EXISTS (SELECT 1 FROM users WHERE personne_id=@id)",
                            new Dictionary<string, object> { { "@id", createdPersonneId.Value } });
                    }
                    catch
                    {
                        // Best effort cleanup only.
                    }
                }

                ViewBag.Error = "Inscription impossible : " + ex.Message;
                return View();
            }
        }

        private bool HasPersonneColumn(string columnName)
        {
            var count = _db.ExecuteScalar(
                "SELECT COUNT(1) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='personne' AND COLUMN_NAME=@column",
                new Dictionary<string, object> { { "@column", columnName } });
            return Convert.ToInt32(count ?? 0) > 0;
        }

        private int? ResolveLocaliteId(string localiteInput)
        {
            if (string.IsNullOrWhiteSpace(localiteInput))
            {
                return null;
            }

            if (int.TryParse(localiteInput, out var parsedId))
            {
                var exists = Convert.ToInt32(_db.ExecuteScalar(
                    "SELECT COUNT(1) FROM localite WHERE id=@id",
                    new Dictionary<string, object> { { "@id", parsedId } }) ?? 0) > 0;
                return exists ? parsedId : null;
            }

            var existingId = _db.ExecuteScalar(
                "SELECT TOP 1 id FROM localite WHERE intitule=@intitule",
                new Dictionary<string, object> { { "@intitule", localiteInput } });

            if (existingId != null && existingId != DBNull.Value)
            {
                return Convert.ToInt32(existingId);
            }

            var createdId = _db.ExecuteScalar(
                "INSERT INTO localite (intitule) VALUES (@intitule); SELECT CAST(SCOPE_IDENTITY() AS int);",
                new Dictionary<string, object> { { "@intitule", localiteInput } });

            if (createdId == null || createdId == DBNull.Value)
            {
                return null;
            }

            return Convert.ToInt32(createdId);
        }
    }
}
