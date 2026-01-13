using AddWebsiteMvc.Business.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Entities
{
    public class Voter
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Reference { get; set; }
        public BallotStatus? BallotStatus { get; set; } = Enums.BallotStatus.Pending;
        public int? CategoryId { get; set; }
        public virtual Category? Category { get; set; }
    }
}
