using AddWebsiteMvc.Business.Entities.Identity;
using AddWebsiteMvc.Business.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Entities
{
    public class PaymentLog
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public string Reference { get; set; } = default!;
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public Guid BallotId { get; set; }
        public virtual Ballot Ballot { get; set; } = default!;
        public DateTime CreatedDate { get; set; }
        public DateTime? ConfirmedDate { get; set; }
        public string? AuthorizationUrl { get; set; }
        public string? AccessCode { get; set; }
        public int? RetryCount { get; set; } = 0;
    }
}
