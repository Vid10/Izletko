namespace Projekt.Models
{
    public class Uporabnik
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string GesloHash { get; set; } = string.Empty;
    }
}
