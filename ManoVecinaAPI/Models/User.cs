namespace ManoVecinaAPI.Models;

public class User
{
    public int Id { get; set; }

    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Role { get; set; } = "client"; // client | worker | admin
    public string PasswordHash { get; set; } = default!;

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsAvailable { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ───────────────────────────────
    //           Relaciones
    // ───────────────────────────────

    // Un usuario puede tener varios gigs si es trabajador
    public ICollection<Gig> Gigs { get; set; } = new List<Gig>();

    // Un usuario puede ser cliente en varias órdenes
    public ICollection<Order> OrdersAsClient { get; set; } = new List<Order>();

    // Un usuario puede ser trabajador en varias órdenes
    public ICollection<Order> OrdersAsWorker { get; set; } = new List<Order>();

    // Reviews escritas POR este usuario (FromUser)
    public ICollection<Review> ReviewsFrom { get; set; } = new List<Review>();

    // Reviews recibidas POR este usuario (ToUser)
    public ICollection<Review> ReviewsTo { get; set; } = new List<Review>();
}