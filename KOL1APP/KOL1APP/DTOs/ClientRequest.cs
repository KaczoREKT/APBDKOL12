using System.ComponentModel.DataAnnotations;

namespace KOL1APP.DTOs
{
    public class ClientRequest
    {
        
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }
        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }
        [Required]
        [MaxLength(100)]
        public string Address { get; set; }
    }
}