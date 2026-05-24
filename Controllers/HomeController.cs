using Microsoft.AspNetCore.Mvc;
using Inventaire.Data;

namespace Inventaire.Controllers
{
    public class HomeController : BaseController
    {
        private readonly DbHelper _db;
        public HomeController(DbHelper db) { _db = db; }

        public IActionResult Index()
        {
            var check = CheckAccess(IsViewer());
            if (check != null) return check;
            SetUserViewBag();

            try
            {
                ViewBag.TotalProduits = _db.ExecuteScalar("SELECT COUNT(*) FROM produits") ?? 0;
                ViewBag.TotalCategories = _db.ExecuteScalar("SELECT COUNT(*) FROM categories") ?? 0;
                ViewBag.TotalFournisseurs = _db.ExecuteScalar("SELECT COUNT(*) FROM fournisseurs") ?? 0;
                ViewBag.TotalEntrees = _db.ExecuteScalar("SELECT COUNT(*) FROM entrees") ?? 0;
                ViewBag.TotalSorties = _db.ExecuteScalar("SELECT COUNT(*) FROM sorties") ?? 0;
                ViewBag.TotalStock = _db.ExecuteScalar("SELECT ISNULL(SUM(qte_stock),0) FROM stock") ?? 0;

                // Recent entrees
                ViewBag.RecentEntrees = _db.ExecuteQuery(@"
                    SELECT TOP 5 e.id, e.date_entree, e.qte_entree, p.label AS produit, f.raison_social AS fournisseur
                    FROM entrees e
                    LEFT JOIN produits p ON e.produit_id = p.produit_id
                    LEFT JOIN fournisseurs f ON e.fournisseur_id = f.fournisseur_id
                    ORDER BY e.date_entree DESC");

                // Recent sorties
                ViewBag.RecentSorties = _db.ExecuteQuery(@"
                    SELECT TOP 5 s.id, s.date_sortie, s.qte_sortie, p.label AS produit, per.nom + ' ' + per.prenom AS personne
                    FROM sorties s
                    LEFT JOIN produits p ON s.produit_id = p.produit_id
                    LEFT JOIN personne per ON s.personne_id = per.personne_id
                    ORDER BY s.date_sortie DESC");

                // Low stock alerts
                ViewBag.LowStock = _db.ExecuteQuery(@"
                    SELECT TOP 5 s.id, s.qte_stock, p.label
                    FROM stock s
                    LEFT JOIN produits p ON s.id = p.produit_id
                    WHERE s.qte_stock < 20
                    ORDER BY s.qte_stock ASC");
            }
            catch
            {
                // DB might not be set up yet – silently ignore
            }

            return View();
        }
    }
}
