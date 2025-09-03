
namespace Stripe.Onboarding.Foundations.Common.Models
{

    public class BasePage
    {
        public string PageTitle { get; set; }
        public string SiteName { get; set; }
        public Guid UserId { get; set; }
        public int CartItems { get; set; }
        public List<NavigationItem> TopNavigation { get; set; }
        public BasePage() { }
    }
}
