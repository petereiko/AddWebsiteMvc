using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Models.Election
{
    public class VotePriceDto
    {
        public Guid Id { get; set; }
        public decimal Price { get; set; }
        public ElectionDto? Election { get; set; } = default!;
        public bool IsActive { get; set; }
    }
}
