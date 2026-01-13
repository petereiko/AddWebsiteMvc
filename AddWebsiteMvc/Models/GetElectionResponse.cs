using AddWebsiteMvc.Business.Models.Election;

namespace AddWebsiteMvc.Models
{

    public class GetElectionResponse:BaseResponse
    {
        public ElectionDto Election { get; set; } = default!;
    }


}
