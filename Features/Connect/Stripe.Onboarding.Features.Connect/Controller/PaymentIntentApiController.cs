using Microsoft.AspNetCore.Mvc;
using Stripe.Onboarding.Features.Payments.Models.Data;
using Stripe.Onboarding.Foundations.Common.Models;
using Stripe.Onboarding.Foundations.Integrations.Stripe.Services;

namespace Stripe.Onboarding.Features.Payment.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentIntentApiController : ControllerBase
    {
        IStripeCheckoutService _stripeService { get; set; }
        public PaymentIntentApiController(IStripeCheckoutService stripeService)
        {
            _stripeService = stripeService;
        }

        private int CalculateOrderAmount(IEnumerable<ProductItem> items)
        {
            // Replace this constant with a calculation of the order's amount
            // Calculate the order total on the server to prevent
            // people from directly manipulating the amount on the client
            return 1400;
        }

    }
}
