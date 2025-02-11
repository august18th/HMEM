using HMEM.Common.Models;
using HMEM.Data;
using Microsoft.AspNetCore.Mvc;

namespace HMEM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PricesController : ControllerBase
    {
        private readonly CryptoPriceRepository _repository;

        public PricesController(CryptoPriceRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public async Task<IActionResult> AddPrice([FromBody] PriceEntry price)
        {
            await _repository.SavePriceAsync(price);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetPrices()
        {
            var prices = await _repository.GetPricesAsync();
            return Ok(prices);
        }
    }
}
