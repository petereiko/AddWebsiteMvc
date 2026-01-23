using AddWebsiteMvc.Business.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Entities
{
    public class Ballot
    {
        public Guid Id { get; set; }
        public Guid VoterId { get; set; }
        public virtual Voter Voter { get; set; } = default!;

        public int Count { get; set; }
        public BallotStatus Status { get; set; } = BallotStatus.Pending;

        public Guid CandidateId { get; set; }
        public virtual Candidate Candidate { get; set; } = default!;

        public string Reference { get; set; } = default!;

        public int CategoryId { get; set; }
        public virtual Category Category { get; set; } = default!;

        public DateTime VoteDate { get; set; }

        public DateTime CastTime { get; set; } = DateTime.UtcNow;
        public bool IsFree { get; set; }
    }
}
