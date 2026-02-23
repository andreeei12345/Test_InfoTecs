using Microsoft.AspNetCore.Mvc;
using Test_InfoTecs.Models.DB;
using Test_InfoTecs.Services;


namespace Test_InfoTecs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimescaleController : ControllerBase
    {
        private readonly ITimescaleService _service;   

        public TimescaleController(ITimescaleService service) 
        {
            _service = service;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadCsv( IFormFile file )
        {
            if (file == null || file.Length == 0)
                return BadRequest("Файл не предоставлен.");
            try
            {
                await _service.ProcessFileAsync(file);
                return Ok("Файл успешно загружен и обработан.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("results")]
        public async Task<IActionResult> GetFilteredResults([FromQuery] ResultsFilter filter)
        {
            var results = await _service.GetFilteredResultsAsync(filter);

            if (!results.Any())
                return NotFound("Записи по заданным фильтрам не найдены.");

            return Ok(results);
        }

        [HttpGet("last-values")]
        public async Task<IActionResult> GetLastValues([FromQuery] string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return BadRequest("Не указано имя файла");

            var values = await _service.GetLastValuesAsync(fileName);

            if (!values.Any())
                return NotFound("Данные для указанного файла не найдены");

            return Ok(values);
        }
    }
}