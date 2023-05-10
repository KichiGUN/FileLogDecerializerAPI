using System.ComponentModel.DataAnnotations;

namespace FileLogDecerializerAPI.Models
{
    public class Scan
    {
        [Required]
        public DateTime scanTime { get; set; }
        [Required]
        public string db { get; set; }
        [Required]
        public string server { get; set; }
        public int errorCount { get; set; }
    }
}
