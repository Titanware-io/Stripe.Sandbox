using Microsoft.AspNetCore.Mvc;
using Stripe.Onboarding.Foundations.Integrations.Stripe.Services;
using Stripe.Onboarding.Foundations.Products.Services;

namespace Stripe.Onboarding.Features.Cart.Controller
{
    [Route("v1/api/connectsession")]
    [ApiController]
    public class ConnectSessionApiController : ControllerBase
    {
        IStripeConnectService _stripeService { get; set; }
        public ConnectSessionApiController(IStripeConnectService stripeService)
        {
            _stripeService = stripeService;
        }
        [HttpPost("account/{accountId}")]
        public async Task<IActionResult> GetConnectSession([FromRoute] string accountId)
        { 
            var session = await _stripeService.CreateAccountSession(accountId);

            return Ok(new { session.ClientSecret });
        }
        [HttpPost("account/{accountId}/status")]
        public async Task<IActionResult> GetConnectOnboardingStatus([FromRoute] string accountId)
        {
            var account = await _stripeService.GetAccount(accountId);
            return Ok(new { status = _stripeService.IsAccountOnboarded(account) });
        }
    }
}
