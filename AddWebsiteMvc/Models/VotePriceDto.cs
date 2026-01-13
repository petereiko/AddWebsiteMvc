namespace AddWebsiteMvc.Models
{
    public class VotePriceDto
    {
        public Guid Id { get; set; }
        public decimal Price { get; set; }
        public Election? Election { get; set; } = default!;
        public bool IsActive { get; set; }
    }
}
