namespace FileLogDecerializerAPI.Models
{
    internal class ServerInfoDto
    {
        public string AppName { get; set; }
        public string Version { get; set; }
        public DateTime DateUtc { get; set; }
    }
}