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

///TODO newErrors - необходимо разобраться почему возникает ошибка при десериализации переданных данных
///TODO рефакторинг кода (как минимум errors/{id} надо исправить чтоб под условия тз подходил и все дубли кода исправить надо)

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
                var entity = JsonConvert.DeserializeObject<Entity>(r.ReadToEnd()); //Позже перепишу это извращение, хочу сегодня все методы сделать, а потом уже делать их красивыми
                
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
        public List<ErrorsDto> GetErrors()
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

                return fileDtos;
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

        [HttpGet("errors/1")]
        public ErrorsDto GetErrorsForIndex(int index)
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

                if (fileDtos.Count() - 1 <= index)
                    return null;
                else
                {
                    return fileDtos.ElementAt(index);
                }
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
        public String PostRequestAsync(JObject json) 
        {
            var scan = JsonConvert.SerializeObject(json);
            return scan;

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
