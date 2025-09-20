using BookCenter.Application.DTOs.BorrowRecords;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Application.Abstractions.Services
{
    public interface IBorrowRecordService
    {
        Task BorrowBookAsync(CreateBorrowRecordDto dto);
        Task ReturnBookAsync(int readerId, int bookId);
        Task<IEnumerable<GetBorrowRecordDto>> GetBorrowedBooksAsync(int readerId);
        Task<IEnumerable<GetBorrowRecordDto>> GetOverdueBooksAsync();
    }
}
