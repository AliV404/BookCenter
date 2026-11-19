using BookCenter.Application.Abstractions.Services;
using BookCenter.Application.Common;
using BookCenter.Application.DTOs.Publishers;
using Microsoft.AspNetCore.Mvc;

namespace BookCenter.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublishersController : ControllerBase
    {
        private readonly IPublisherService _service;

        public PublishersController(IPublisherService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<GetPublisherItemDto>>> GetAll([FromQuery] PaginationQuery query)
        {
            return Ok(await _service.GetAllAsync(query));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<GetPublisherDto>> GetById(int id)
        {
            return Ok(await _service.GetByIdAsync(id));
        }

        [HttpPost]
        public async Task<ActionResult<GetPublisherDto>> Create(CreatePublisherDto dto)
        {
            var id = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id }, await _service.GetByIdAsync(id));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdatePublisherDto dto)
        {
            await _service.UpdateAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
