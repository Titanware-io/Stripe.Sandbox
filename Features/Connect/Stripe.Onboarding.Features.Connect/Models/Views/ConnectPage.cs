using Accelerate.Foundations.Common.Models.Components;
using Stripe.Onboarding.Foundations.Common.Models;
using Stripe.Onboarding.Foundations.Common.Models.Components;
using Stripe.Onboarding.Foundations.Common.Models.Components.Form;

namespace Stripe.Onboarding.Features.Connect.Models.Views
{
    public class ConnectPage : BasePage
    {
        public ConnectPage(BasePage basePage)
        {
            this.SiteName = basePage.SiteName;
            this.CartItems = basePage.CartItems;
            this.UserId = basePage.UserId;
            this.PageTitle = basePage.PageTitle;
            this.TopNavigation = basePage.TopNavigation;
        }
        public string AccountId { get; set; }
        public string PublicKey { get; set; }
        public string ClientSecret { get; set; }
        public string PostbackUrl { get; set; }
        public string RedirectUrl { get; set; }

        public string Status { get; set; }
        public string CustomerEmail { get; set; }
        public Form Form { get; set; }

        public string StripeAccountLoginUrl { get; set; }
        public string? DashboardComponent { get; set; }

        public NavigationGroup PageTabs { get; set; }
        public AjaxTable<object> AccountsTable { get; set; }
        public List<CardNavigationItem> PageCards { get; set; }
    }
}
