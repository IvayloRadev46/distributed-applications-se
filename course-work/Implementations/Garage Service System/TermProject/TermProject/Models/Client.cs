using System.ComponentModel.DataAnnotations;

public class Client
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [MaxLength(15)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Email { get; set; }

    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

    public long LoyaltyPoints { get; set; } = 0;

    public ICollection<Car> Cars { get; set; } = new List<Car>();
}
