using System.ComponentModel.DataAnnotations;

namespace FileLogDecerializerAPI.Models
{
    public class Errors
    {
        [Required]
        public string module { get; set; }
        [Required]
        public int ecode { get; set; }
        [Required]
        public string error { get; set; }
    }
}
