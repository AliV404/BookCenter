using BookCenter.Application.Abstractions.Services;
using BookCenter.Application.DTOs.Categories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BookCenter.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _service;

        public CategoriesController(ICategoryService service)
        {
            _service = service;
        }
        [HttpGet]
        public async Task<IActionResult> Get(int page = 1, int take = 10)
        {
            return Ok(await _service.GetAllAsync(page, take));
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            if (id < 1) return BadRequest();
            var categoryDto = await _service.GetByIdAsync(id);
            if (categoryDto == null) return NotFound();
            return Ok(categoryDto);
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoyDto dto)
        {
            await _service.CreateAsync(dto);
            return Ok();
        }
        [HttpPut]
        public async Task<IActionResult> Update(int id, UpdateCategoryDto dto)
        {
            if (id < 0) return BadRequest();
            await _service.UpdateAsync(id, dto);
            return Ok();
        }
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            if (id < 0) return BadRequest();
            await _service.DeleteAsync(id);
            return Ok();
        }
    }
}
