using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AddWebsiteMvc.Business.Enums;

namespace AddWebsiteMvc.Business.Entities.Identity
{
    [NotMapped]
    public class TransactionModel
    {
        public string Id { get; set; }
        public string Reference { get; set; }
        public string Voter { get; set; }
        public string Contestants { get; set; }
        public DateTime VoteDate { get; set; }
        public BallotStatus Status { get; set; }
        public int Count { get; set; }
    }
}
