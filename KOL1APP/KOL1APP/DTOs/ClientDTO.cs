using KOL1APP.DTOs;

public class ClientDTO
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Address { get; set; }
    public List<CarRentalDTO> Rentals { get; set; }
}