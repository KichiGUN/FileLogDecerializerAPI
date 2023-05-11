using FileLogDecerializerAPI.Models;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization;
using System.Reflection;
using System.Net;
using System.Web;
using System.Threading;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;




namespace FileLogDecerializerAPI.Controllers
{
    [Route("api/")]
    [ApiController]
    public class AllDataController : ControllerBase
    {
        private const string filePath = "Data/data.json"; //Путь до основного файла с данными
        private Entity entity = null;

        [HttpGet("allData")]
        public Entity Get()// Запрос на выдачу файла с данными без изменений
        {
            return this.FileReader();
        }

        [HttpGet("scan")]
        public Scan GetScans() // Запрос на блок scan из JSON-а
        {
            return this.FileReader().scan;
        }

        [HttpGet("filenames")] 
        public List<string> GetCorrectFiles(Boolean value) // Массив со всеми файлами, который прошли без ошибки или с ошибкой
        {
            return this.FileReader().files.Where(x => x.result == value).Select(x => x.filename).ToList();
        }
       
       [HttpGet("errors")] 
        public List<ErrorsDto> GetErrors(int? index) // Может как принимать параметр, так и работать без него. В случае, если параметр указан, то передаются все ошибочные файлы, если указан индекс, то 1
        {
            var filesWithError = this.FileReader().files.Where(x => x.result == false && x.errors.Count() > 0).ToList();

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
                        throw new Exception("Ошибок меньше, чем указано в индексе. Ошибок: " + fileDtos.Count());

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
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        [HttpGet("errors/Count")]
        public int GetErrorsCount() // Выводит число ошибок в файле
        {
            return this.FileReader().scan.errorCount;
        }

        [HttpGet("query/check")] 
        public QueryCheckDto GetQueryCheck() // выводит все ошибкb файлов, начинающихся с query_
        {
            Regex regex = new Regex("^query_", RegexOptions.IgnoreCase);

            var tmpFiles = this.FileReader().files.Where(x => regex.IsMatch(x.filename)).ToList();

            var list = tmpFiles.Where(x => x.result == false).Select(x => x.filename).ToList();

            var queryCheckDto = new QueryCheckDto
            {
                total = tmpFiles.Count(),
                correct = tmpFiles.Where(x => x.result == true).Count(),
                errors = tmpFiles.Where(x => x.result == false).Count()
            };
            if(list.Count() > 0)
                queryCheckDto.filenames = list;

            return queryCheckDto;
        }

        [HttpPost("newErrors")]
        public async Task<IActionResult> ReceiveJsonAsync(JsonValue json) // Запрос создает JSON - файл в папку Data
        {
            Entity entity = null;
            try
            {
                entity = json.Deserialize<Entity>();
                if (entity.scan == null || entity.files == null)
                    throw new Exception("Неверный формат JSON-файла");

                string filePath = Path.Combine("Data", DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss") + ".json");

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
        public IActionResult GetServerInfo() // Предоставление информации о сервере
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

        private Entity FileReader() // Метод для упрощения чтения из основного файла
        {
            if(entity == null)
            {
                using (StreamReader r = new StreamReader(filePath))
                {
                    entity = JsonConvert.DeserializeObject<Entity>(r.ReadToEnd());
                }
            }
            
            return entity;
        }
    }
}
