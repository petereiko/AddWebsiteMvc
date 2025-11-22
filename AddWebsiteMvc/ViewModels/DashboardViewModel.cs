using AddWebsiteMvc.Models;

namespace AddWebsiteMvc.ViewModels
{
    public class DashboardViewModel
    {
        public int ContestantCount { get; set; }
        public Election? Election { get; set; }
    }
}
