using System.ComponentModel.DataAnnotations;

namespace FileLogDecerializerAPI.Models
{
    public class Entity
    {
        [Required]
        public Scan scan { get; set; }
        [Required]
        public Files[] files { get; set; }
    }
}
