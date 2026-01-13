using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Models
{
    public class BaseResult
    {
        public bool Succeeded { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public List<string> Errors { get; set; } = new();

        public static BaseResult Success() => new() { Succeeded = true };
        public static BaseResult Failure(List<string> errors) => new() { Succeeded = false, Errors = errors };
    }

    public class BaseResult<T> : BaseResult
    {
        public T? Data { get; set; }
        public DateTime StartDate {  get; set; }
        public DateTime CloseDate { get; set; }
        public decimal UnitVotePrice { get; set; }
        public int VotesCastToday { get; set; }
        public string ElectionTitle { get; set; } = default!;
        public static BaseResult<T> Success(T data) => new() { Succeeded = true, Data = data };
        public static new BaseResult<T> Failure(List<string> errors) => new() { Succeeded = false, Errors = errors };
        public string Message { get; set; } = default!;
    }
}
