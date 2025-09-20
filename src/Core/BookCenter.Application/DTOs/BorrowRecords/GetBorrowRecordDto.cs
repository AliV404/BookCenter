using BookCenter.Application.DTOs.Books;
using BookCenter.Application.DTOs.Readers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Application.DTOs.BorrowRecords
{
    public record GetBorrowRecordDto
        (
        int Id,
        int BookId,
        GetBookItemDto Book,
        int ReaderId,
        GetReaderItemDto Reader,
        DateTime BorrowedAt,
        DateTime DueDate,
        DateTime? ReturnedAt,
        bool IsOverdue
        );
    
    
}
