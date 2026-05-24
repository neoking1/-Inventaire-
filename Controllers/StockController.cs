using Microsoft.AspNetCore.Mvc;
using Inventaire.Data;
using System.Data;

namespace Inventaire.Controllers
{
    public class StockController : BaseController
    {
        private readonly DbHelper _db;
        public StockController(DbHelper db) { _db = db; }

        public IActionResult Index()
        {
            var check = CheckAccess(IsViewer());
            if (check != null) return check;
            SetUserViewBag();

            var data = _db.ExecuteQuery(@"
                SELECT s.id AS stock_id, s.qte_stock,
                       p.produit_id, p.label,
                       c.nom_categorie,
                       l.intitule AS localite
                FROM stock s
                LEFT JOIN produits p ON s.id = p.produit_id
                LEFT JOIN categories c ON p.categorie_id = c.id
                LEFT JOIN localite l ON l.stock = s.id
                ORDER BY s.id");

            var total = data.Rows.Count > 0
                ? data.AsEnumerable().Sum(r => Convert.ToInt32(r["qte_stock"] == DBNull.Value ? 0 : r["qte_stock"]))
                : 0;

            ViewBag.TotalUnites = total;
            ViewBag.TotalStock = data.Rows.Count;
            ViewBag.AlertesStock = data.AsEnumerable().Count(r => Convert.ToInt32(r["qte_stock"] == DBNull.Value ? 0 : r["qte_stock"]) < 20);

            return View(data);
        }
    }
}


