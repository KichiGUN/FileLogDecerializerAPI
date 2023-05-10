using FileLogDecerializerAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace FileLogDecerializerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AllDataController : ControllerBase
    {
        private const string filePath = "Data/data.json";
        [HttpGet]
        public Entity Get()
        {
            using (StreamReader r = new StreamReader(filePath))
            {
                var entitys = JsonSerializer.Deserialize<Entity>(r.ReadToEnd());
                return entitys;
            }
        }
    }
}
