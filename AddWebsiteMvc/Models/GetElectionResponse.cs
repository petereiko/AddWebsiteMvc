namespace AddWebsiteMvc.Models
{

    public class GetElectionResponse:BaseResponse
    {
        public Election data { get; set; } = default!;
    }


}
