using BookCenter.Application.Abstractions.Services;
using BookCenter.Application.DTOs.Publishers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BookCenter.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PublishersController : ControllerBase
    {
        private readonly IPublihserService _service;

        public PublishersController(IPublihserService service)
        {
            _service = service;
        }
        [HttpGet]
        public async Task<IActionResult> Get(int page=1, int take = 10)
        {
            return Ok(await _service.GetAllAsync(page, take));
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await _service.GetByIdAsync(id));
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromForm]CreatePublisherDto publisherDto)
        {
            await _service.CreateAsync(publisherDto);
            return Created();
        }
        [HttpPut]
        public async Task<IActionResult> Update(int id, UpdatePublisherDto publisherDto)
        {
            await _service.UpdateAsync(id, publisherDto);
            return NoContent();
        }
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok();
        }

    }
}
