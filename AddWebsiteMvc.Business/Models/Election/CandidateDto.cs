using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Models.Election
{
    public class CandidateDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = default!;
        public int Order { get; set; }
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public Guid ElectionId { get; set; }
        public ElectionDto Election { get; set; } = default!;
        public string? PassportFileName { get; set; }
        public IFormFile? PassportFile { get; set; }
        public int CandidateCategoryCount { get; set; }
        public bool IsActive { get; set; }
        public int VoteCount { get; set; }
        public decimal? votePrice { get; set; }
        public int StateId { get; set; }
        public string StateName { get; set; }

        public List<string> Errors { get; set; } = new();
        public List<SelectListItem> States { get; set; } = new();
    }
}
