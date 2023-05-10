using System.ComponentModel.DataAnnotations;

namespace FileLogDecerializerAPI.Models
{
    public class Files
    {
        [Required]
        public string filename { get; set; }
        [Required]
        public bool result { get; set; }
        public Errors[] errors { get; set; }
        [Required]
        public DateTime scantime { get; set; }
    }
}
