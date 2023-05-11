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
        public async Task<IActionResult> Get()// Запрос на выдачу файла с данными без изменений
        {
            try
            {
                var data = await Task.Run(() => FileReader());
                if (data == null) return NotFound();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("scan")]
        public async Task<IActionResult> GetScans() // Запрос на блок scan из JSON-а
        {
            try
            {
                var data = await Task.Run(() => FileReader().scan);
                if (data == null) return NotFound();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("filenames")] 
        public async  Task<IActionResult> GetCorrectFiles(Boolean? value) // Массив со всеми файлами, который прошли без ошибки или с ошибкой
        {
            try
            {
                var data = await Task.Run(() => FileReader().files.Where(x => x.result == value).Select(x => x.filename).ToList());
               
                if (data.Count() == 0) return NotFound();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("errors")]
        public async Task<IActionResult> GetErrors(int? index) // Может как принимать параметр, так и работать без него. В случае, если параметр указан, то передаются все ошибочные файлы, если указан индекс, то 1
        {
            var filesWithError = await Task.Run(() => FileReader().files.Where(x => x.result == false && x.errors.Count() > 0).ToList());
            
            if(filesWithError == null) return NotFound();

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
            if (index.HasValue)
            {
                if (index.Value >= fileDtos.Count())
                    return BadRequest("Индекс вышел за максимальное количество. Макс. количество - " + fileDtos.IndexOf(fileDtos[^1]));

                return Ok(new List<ErrorsDto> { fileDtos[index.Value] });
            }
            else
            {
                return Ok(fileDtos);
            }
        }

        [HttpGet("errors/Count")]
        public async Task<IActionResult> GetErrorsCount() // Выводит число ошибок в файле
        {
            try
            {
                var data = await Task.Run(() => FileReader().scan);
                if (data == null) return NotFound();

                return Ok(data.errorCount);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("query/check")] 
        public async Task<IActionResult> GetQueryCheck() // выводит все ошибкb файлов, начинающихся с query_
        {
            Regex regex = new Regex("^query_", RegexOptions.IgnoreCase);

            var tmpFiles = await Task.Run(() => FileReader().files.Where(x => regex.IsMatch(x.filename)).ToList());

            if (tmpFiles.Count == 0) return NotFound();

            var list = tmpFiles.Where(x => x.result == false).Select(x => x.filename).ToList();

            var queryCheckDto = new QueryCheckDto
            {
                total = tmpFiles.Count(),
                correct = tmpFiles.Where(x => x.result == true).Count(),
                errors = tmpFiles.Where(x => x.result == false).Count()
            };

            if(list.Count() > 0)
                queryCheckDto.filenames = list;

            return Ok(queryCheckDto);
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
