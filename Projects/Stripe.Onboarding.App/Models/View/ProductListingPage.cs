using Stripe.Onboarding.Features.Cart.Models.Data;
using Stripe.Onboarding.Foundations.Common.Models;

namespace Stripe.Onboarding.App.Models.View
{
    public class ProductListingPage : BasePage
    {
        public ProductListingPage(BasePage page)
        {
            this.TopNavigation = page.TopNavigation;
            this.UserId = page.UserId;
        }
        public SessionCart Cart { get; set; }
        public List<ProductItem> Catalog { get; set; }
        public string CartPostbackUrl { get; set; }
    }
}
