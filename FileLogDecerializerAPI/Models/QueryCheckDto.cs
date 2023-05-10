namespace FileLogDecerializerAPI.Models
{
    public class QueryCheckDto
    {
        public int total { get; set; }
        public int correct { get; set; }
        public int errors { get; set; }
        public List<string> filenames { get; set; }
    }
}
