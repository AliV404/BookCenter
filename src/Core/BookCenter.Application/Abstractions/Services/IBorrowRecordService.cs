using BookCenter.Application.Common;
using BookCenter.Application.DTOs.BorrowRecords;

namespace BookCenter.Application.Abstractions.Services
{
    public interface IBorrowRecordService
    {
        Task<PagedResult<GetBorrowRecordDto>> GetAllAsync(BorrowRecordQuery query);
        Task<GetBorrowRecordDto> GetByIdAsync(int id);
        Task<int> BorrowBookAsync(CreateBorrowRecordDto dto);
        Task ReturnBookAsync(int recordId);
    }
}
