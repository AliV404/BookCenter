using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Domain.Base
{
    public class BaseNameableEntity:BaseEntity
    {
        public string Name { get; set; }
    }
}
