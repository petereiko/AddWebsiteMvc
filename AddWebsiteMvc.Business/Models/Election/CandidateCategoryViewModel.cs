using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Models.Election
{
    public class CandidateCategoryViewModel
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsChecked { get; set; }
    }

    public class CandidateCategoryListViewModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public Guid CandidateId { get; set; }
        public List<CheckboxListItem> Categories { get; set; } = new List<CheckboxListItem>();
    }

    public class CheckboxListItem
    {
        public bool IsChecked { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; } 
    }
}
