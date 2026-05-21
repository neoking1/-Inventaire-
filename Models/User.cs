namespace Inventaire.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Mdp { get; set; }
        public string Type { get; set; }
        public int PersonneId { get; set; }
    }
}