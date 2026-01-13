using AddWebsiteMvc.Business.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Entities
{
    public class Candidate:BaseEntity
    {
        public string Title { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public Guid ElectionId { get; set; }
        public virtual Election Election { get; set; } = default!;
        public string? PassportFileName { get; set; }
        public int StateId { get; set; }
        public virtual State? State { get; set; }
        public int Order { get; set; }
        public virtual List<CandidateCategory> CandidateCategories { get; set; } = new();

    }
}
