using BookCenter.Application.Abstractions.Services;
using BookCenter.Application.DTOs.BorrowRecords;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BookCenter.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RecordsController : ControllerBase
    {
        private readonly IBorrowRecordService _service;

        public RecordsController(IBorrowRecordService service)
        {
            _service = service;
        }

        [HttpPost("BorrowBook")]
        public async Task<IActionResult> Post([FromForm] CreateBorrowRecordDto dto)
        {
            await _service.BorrowBookAsync(dto);
            return Ok();
        }
        [HttpGet("{readerId}")]
        public async Task<IActionResult> Get(int readerId)
        {
            return Ok(await _service.GetBorrowedBooksAsync(readerId));
        }
        [HttpGet("OverDueRecords")]
        public async Task<IActionResult> Get()
        {
            return Ok(await _service.GetOverdueBooksAsync());
        }
        [HttpPut("ReturnBook")]
        public async Task<IActionResult> Put(int readerId, int bookId)
        {
            await _service.ReturnBookAsync(readerId, bookId);
            return Ok();
        }
    }
}
