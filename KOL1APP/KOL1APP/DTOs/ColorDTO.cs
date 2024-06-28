using System.ComponentModel.DataAnnotations;

namespace KOL1APP.DTOs;

public class ColorDTO
{
    public int Id { get; set; }
    [MaxLength(100)]
    public string Name { get; set; }
}