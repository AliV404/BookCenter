using BookCenter.Application.Abstractions.Services;
using BookCenter.Application.Common;
using BookCenter.Application.DTOs.Authors;
using Microsoft.AspNetCore.Mvc;

namespace BookCenter.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorService _service;

        public AuthorsController(IAuthorService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<GetAuthorItemDto>>> GetAll([FromQuery] PaginationQuery query)
        {
            return Ok(await _service.GetAllAsync(query));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<GetAuthorDto>> GetById(int id)
        {
            return Ok(await _service.GetByIdAsync(id));
        }

        [HttpPost]
        public async Task<ActionResult<GetAuthorDto>> Create(CreateAuthorDto dto)
        {
            var id = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id }, await _service.GetByIdAsync(id));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateAuthorDto dto)
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
