using System.ComponentModel.DataAnnotations;
using KOL1APP.DTOs;

public class ClientDTO
{
    public int Id { get; set; }
    [MaxLength(50)]
    public string FirstName { get; set; }
    [MaxLength(100)]
    public string LastName { get; set; }
    [MaxLength(100)]
    public string Address { get; set; }
    public List<CarRentalDTO> Rentals { get; set; }
}