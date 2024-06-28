using System.ComponentModel.DataAnnotations;

namespace KOL1APP.DTOs;

public class CarDTO
{
    public int Id { get; set; }
    
    [MaxLength(17)]
    public string VIN { get; set; }
    [MaxLength(100)]
    public string Name { get; set; }
    public int Seats { get; set; }
    public int PricePerDay { get; set; }
    public int ModelId { get; set; }
    public int ColorId { get; set; }
    public ModelDTO Model { get; set; }
    public ColorDTO Color { get; set; }
}