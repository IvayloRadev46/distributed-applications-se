namespace Garage.WebClient.Models;

public class RepairModel
{
    public int Id { get; set; }

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string Status { get; set; } = "Scheduled";

    public int CarId { get; set; }

    public string? CarModel { get; set; }
}
