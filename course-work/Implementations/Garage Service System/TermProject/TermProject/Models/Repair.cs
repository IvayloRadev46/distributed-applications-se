using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class Repair
{
    public int Id { get; set; }

    [MaxLength(250)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public decimal Price { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Scheduled";

    public int CarId { get; set; }

    [JsonIgnore]
    public Car? Car { get; set; }
}
