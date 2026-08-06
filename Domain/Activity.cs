
using System.ComponentModel.DataAnnotations;

namespace Domain;

public class Activity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [Required]
    public required string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    [Required]
    public required string Description { get; set; } = string.Empty;
    [Required]
    public required string Category { get; set; } = string.Empty;
    public bool IsCancelled { get; set; }

    //location props
    public string City { get; set; } = string.Empty;
    public string Venue { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }

}
