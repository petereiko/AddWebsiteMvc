using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Models
{
    public class MessageResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class MessageResult<T> : MessageResult
    {
        public T? Data { get; set; }
    }
}
