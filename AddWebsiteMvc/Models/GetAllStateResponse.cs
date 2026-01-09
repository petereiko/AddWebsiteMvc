namespace AddWebsiteMvc.Models
{
    public class GetAllStateResponse: BaseResponse
    {
        public List<StateDto> data { get; set; } = new();
    }
}
