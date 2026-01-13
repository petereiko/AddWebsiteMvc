using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Common
{
    public class Result<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();

        public static Result<T> Success(T data, string message = "")
            => new() { IsSuccess = true, Data = data, Message = message };

        public static Result<T> Failure(string message, List<string> errors = null)
            => new() { Message = message, Errors = errors ?? new() };
    }
}
