namespace Inventaire.Models
{
    public class Fournisseur
    {
        public int FournisseurId { get; set; }
        public string RaisonSocial { get; set; }
        public string Contact { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string Adress { get; set; }
        public string Ville { get; set; }
        public string Remarque { get; set; }
    }
}