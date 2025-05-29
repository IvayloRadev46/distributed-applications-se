using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class Car
{
    public int Id { get; set; }

    [Required]
    [MaxLength(17)]
    public string VIN { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string LicensePlate { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string Model { get; set; } = string.Empty;

    [Required]
    [Range(1900, 2100)]
    public int ManufactureYear { get; set; }

    [Required]
    public DateTime FirstRegistrationDate { get; set; } = DateTime.Now;
    public int ClientId { get; set; }

    [JsonIgnore]
    public Client? Client { get; set; }

    [JsonIgnore]
    public ICollection<Repair>? Repairs { get; set; }
}
