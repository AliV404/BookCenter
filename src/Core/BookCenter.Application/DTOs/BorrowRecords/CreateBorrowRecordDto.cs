namespace BookCenter.Application.DTOs.BorrowRecords
{
    public class CreateBorrowRecordDto
    {
        public int BookId { get; set; }
        public int ReaderId { get; set; }

        /// <summary>Treated as UTC when no offset is supplied.</summary>
        public DateTime DueDate { get; set; }
    }
}
