using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Application.DTOs.BorrowRecords
{
    public class CreateBorrowRecordDto
    {
        public int BookId { get; set; }
        public int ReaderId { get; set; }
        public DateTime DueDate { get; set; }
    }
}
