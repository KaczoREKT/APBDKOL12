using KOL1APP.DTOs;

public class AddClientRequest
{
    public ClientRequest Client { get; set; }
    public int CarId { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
}