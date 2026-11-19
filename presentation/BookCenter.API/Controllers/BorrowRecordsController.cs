using BookCenter.Application.Abstractions.Services;
using BookCenter.Application.Common;
using BookCenter.Application.DTOs.BorrowRecords;
using Microsoft.AspNetCore.Mvc;

namespace BookCenter.API.Controllers
{
    [Route("api/borrow-records")]
    [ApiController]
    public class BorrowRecordsController : ControllerBase
    {
        private readonly IBorrowRecordService _service;

        public BorrowRecordsController(IBorrowRecordService service)
        {
            _service = service;
        }

        /// <summary>Lists loans. Filter with ?status=active|overdue|returned, ?readerId=, ?bookId=, ?search=</summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<GetBorrowRecordDto>>> GetAll([FromQuery] BorrowRecordQuery query)
        {
            return Ok(await _service.GetAllAsync(query));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<GetBorrowRecordDto>> GetById(int id)
        {
            return Ok(await _service.GetByIdAsync(id));
        }

        /// <summary>Borrows a book: creates a loan and takes one copy off the shelf.</summary>
        [HttpPost]
        public async Task<ActionResult<GetBorrowRecordDto>> Borrow(CreateBorrowRecordDto dto)
        {
            var id = await _service.BorrowBookAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id }, await _service.GetByIdAsync(id));
        }

        /// <summary>Marks a loan as returned and puts the copy back on the shelf.</summary>
        [HttpPost("{id:int}/return")]
        public async Task<IActionResult> Return(int id)
        {
            await _service.ReturnBookAsync(id);
            return NoContent();
        }
    }
}
