using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Domain.Base
{
    public interface IBaseEntity
    {
        int Id { get; set; }
        bool IsDeleted { get; set; }

        string? CreatedBy { get; set; }
        DateTime CreatedAt { get; set; }

        string? ModifiedBy { get; set; }
        DateTime? ModifiedAt { get; set; }

        string? DeletedBy { get; set; }
        DateTime? DeletedAt { get; set; }
    }
}
