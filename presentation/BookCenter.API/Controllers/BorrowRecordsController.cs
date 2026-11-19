using BookCenter.Application.Abstractions.Security;
using BookCenter.Application.Abstractions.Services;
using BookCenter.Application.Common;
using BookCenter.Application.DTOs.BorrowRecords;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookCenter.API.Controllers
{
    /// <summary>
    /// Admins see and manage all loans. Readers may only list/view their own
    /// (any readerId they pass is overridden with the one in their token).
    /// </summary>
    [Route("api/borrow-records")]
    [ApiController]
    public class BorrowRecordsController : ControllerBase
    {
        private readonly IBorrowRecordService _service;
        private readonly ICurrentUser _currentUser;

        public BorrowRecordsController(IBorrowRecordService service, ICurrentUser currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        /// <summary>Lists loans. Filter with ?status=active|overdue|returned, ?readerId=, ?bookId=, ?search=</summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<GetBorrowRecordDto>>> GetAll([FromQuery] BorrowRecordQuery query)
        {
            if (!_currentUser.IsAdmin)
            {
                if (_currentUser.ReaderId is null) return Forbid();
                query.ReaderId = _currentUser.ReaderId;
            }
            return Ok(await _service.GetAllAsync(query));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<GetBorrowRecordDto>> GetById(int id)
        {
            var record = await _service.GetByIdAsync(id);
            // 404 rather than 403 so readers can't probe which loan ids exist.
            if (!_currentUser.IsAdmin && record.ReaderId != _currentUser.ReaderId) return NotFound();
            return Ok(record);
        }

        /// <summary>Borrows a book: creates a loan and takes one copy off the shelf.</summary>
        [Authorize(Roles = Roles.Admin)]
        [HttpPost]
        public async Task<ActionResult<GetBorrowRecordDto>> Borrow(CreateBorrowRecordDto dto)
        {
            var id = await _service.BorrowBookAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id }, await _service.GetByIdAsync(id));
        }

        /// <summary>Marks a loan as returned and puts the copy back on the shelf.</summary>
        [Authorize(Roles = Roles.Admin)]
        [HttpPost("{id:int}/return")]
        public async Task<IActionResult> Return(int id)
        {
            await _service.ReturnBookAsync(id);
            return NoContent();
        }
    }
}
