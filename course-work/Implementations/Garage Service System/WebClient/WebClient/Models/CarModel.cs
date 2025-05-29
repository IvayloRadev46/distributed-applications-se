using System.ComponentModel.DataAnnotations;

namespace Garage.WebClient.Models;

public class CarModel
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
    [Display(Name = "Година на производство")]
    public int ManufactureYear { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Дата на регистрация")]
    public DateTime FirstRegistrationDate { get; set; } = DateTime.Now;


    public int ClientId { get; set; }

    public string? ClientFullName { get; set; }
}
