using System.ComponentModel.DataAnnotations;

namespace Garage.WebClient.Models;

public class ClientModel
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";

    [Required]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime RegistrationDate { get; set; }

    public decimal TotalSpent { get; set; } = 0;
    public int LoyaltyPoints => (int)(TotalSpent / 100) * 10;

    public string FullName => $"{FirstName} {LastName}";
}
