using FileLogDecerializerAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Microsoft.Net.Http.Headers;
using System.Reflection;
using System.Net;
using System.Web;
using System.Text.Json.Serialization;
using System.Threading;
using System.Diagnostics;
using System.Linq;


namespace FileLogDecerializerAPI.Controllers
{
    [Route("api/")]
    [ApiController]
    public class AllDataController : ControllerBase
    {
        private const string filePath = "Data/data.json";
        [HttpGet("allData")]
        public Entity Get()
        {
            using (StreamReader r = new StreamReader(filePath))
            {
                var entity = JsonConvert.DeserializeObject<Entity>(r.ReadToEnd());
                if(entity == null) return null;

                return entity;
            }
        }
        [HttpGet("scan")]
        public Scan GetScans() 
        {
            using (StreamReader r = new StreamReader(filePath))
            {
                var entity = JsonConvert.DeserializeObject<Entity>(r.ReadToEnd()); 
                
                if (entity.scan == null) return null;

                return entity.scan;
            }
        }
        [HttpGet("filenames")]
        public List<string> GetCorrectFiles(Boolean value)
        {
            using (StreamReader r = new StreamReader(filePath))
            {
                var entity = JsonConvert.DeserializeObject<Entity>(r.ReadToEnd());

                var fileNames = entity.files.Where(x => x.result == value).Select(x => x.filename).ToList();

                return fileNames;
            }
        }
       
       [HttpGet("errors")]
        public List<ErrorsDto> GetErrors(int? index)
        {
            using (StreamReader r = new StreamReader(filePath))
            {
                var entity = JsonConvert.DeserializeObject<Entity>(r.ReadToEnd());

                var filesWithError = entity.files.Where(x => x.result == false && x.errors.Count() > 0).ToList();

                var fileDtos = new List<ErrorsDto>();

                foreach (var file in filesWithError)
                {
                    foreach (var error in file.errors)
                    {
                        var fileDto = new ErrorsDto
                        {
                            filename = file.filename,
                            errorDesc = error.error,
                            result = file.result
                        };

                        fileDtos.Add(fileDto);
                    }
                }
                try
                {
                    if (index.HasValue)
                    {
                        List<ErrorsDto> tmpList = new List<ErrorsDto>();
                        if (index.Value >= fileDtos.Count())
                            throw new Exception("В файле находится меньше ошибок, чем указано в индексе");

                        tmpList.Add(fileDtos.ElementAt(Convert.ToInt32(index.Value)));

                        return tmpList;
                    }
                    else
                    {
                        return fileDtos;
                    }
                }
                catch(Exception ex)
                {
                    return null;
                }
            }
        }

        [HttpGet("errors/Count")]
        public int GetErrorsCount()
        {
            using (StreamReader r = new StreamReader(filePath))
            {
                var entity = JsonConvert.DeserializeObject<Entity>(r.ReadToEnd());

                return entity.scan.errorCount;
            }
        }

        [HttpGet("query/check")]
        public QueryCheckDto GetQueryCheck()
        {
            using (StreamReader r = new StreamReader(filePath))
            {
                Regex regex = new Regex("^query_", RegexOptions.IgnoreCase);

                var entity = JsonConvert.DeserializeObject<Entity>(r.ReadToEnd());
                var tmpFiles = entity.files.Where(x => regex.IsMatch(x.filename)).ToList();

                var queryCheckDto = new QueryCheckDto
                {
                    total = tmpFiles.Count(),
                    correct = tmpFiles.Where(x => x.result == true).Count(),
                    errors = tmpFiles.Where(x => x.result == false).Count(),
                    filenames = tmpFiles.Where(x => x.result == false).Select(x => x.filename).ToList()
                };

                return queryCheckDto;
            }
        }

        [HttpPost("newErrors")]
        public async Task<IActionResult> ReceiveJsonAsync(JsonValue json)
        {
            Entity entity = null;
            try
            {
                entity = json.Deserialize<Entity>();
                if (entity.scan == null || entity.files == null)
                    throw new Exception("Неверный формат JSON-файла");

                string filePath = Path.Combine(AppContext.BaseDirectory, "Data", DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss") + ".json");

                await using (StreamWriter file = new StreamWriter(filePath))
                {
                    string jsonModel = JsonConvert.SerializeObject(entity);
                    await file.WriteAsync(jsonModel);
                }
                return Ok();
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Ошибка при сохранении файла: " + ex.Message);
            }
        }

        [HttpGet("service/serviceInfo")]
        public IActionResult GetServerInfo()
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            var dto = new ServerInfoDto
            {
                AppName = assemblyName.Name,
                Version = assemblyName.Version.ToString(),
                DateUtc = DateTime.UtcNow
            };

            return Ok(dto);
        }
    }
}
