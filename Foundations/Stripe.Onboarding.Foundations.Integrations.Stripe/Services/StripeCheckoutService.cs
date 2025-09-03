using Microsoft.Extensions.Options;
using Stripe.Checkout;
using Stripe.Onboarding.Foundations.Integrations.Stripe.Models.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stripe.Onboarding.Foundations.Integrations.Stripe.Services
{
    public class StripeCheckoutService : IStripeCheckoutService
    {
        StripeConfig _config { get; set; }
        IStripeClient _stripeClient { get; set; }
        SessionService _checkoutSessionService { get; set; }
        SetupIntentService _setupIntentService { get; set; }
        PaymentIntentService _paymentIntentService { get; set; }
        
        ChargeService _chargeService { get; set; }
        public StripeCheckoutService(IOptions<StripeConfig> options)
        {
            _config = options.Value;
             
            _stripeClient = new StripeClient(_config.SecretKey);
            
            _checkoutSessionService = new SessionService(_stripeClient);
            _setupIntentService = new SetupIntentService(_stripeClient);
            _paymentIntentService = new PaymentIntentService(_stripeClient);

            _chargeService = new ChargeService(_stripeClient);
        }
        public StripeConfig Config
        {
            get
            {
                return _config;
            }
        }
        public StripeList<Charge> GetCustomerCharges(string customerId, int limit = 10)
        {
            var options = new ChargeListOptions()
            {
                Customer = customerId,
                Limit = 10,
            };
            return _chargeService.List(options);
        }
        public Session GetSessionByIntentId(string intentId)
        {
            var sessions = _checkoutSessionService.List(new SessionListOptions()
            {
                PaymentIntent = intentId,
                Limit = 1
            });
            return sessions?.FirstOrDefault();
        }
        public Session CreateSession(SessionCreateOptions options)
        {
            return _checkoutSessionService.Create(options);
        }
        public Session GetCheckoutSession(string id, SessionGetOptions options)
        {
            return _checkoutSessionService.Get(id, options);
        }
        public Session GetCheckoutSession(string id)
        {
            return _checkoutSessionService.Get(id);
        }
        public SetupIntent GetSetupIntent(string id)
        {
            return _setupIntentService.Get(id);
        }

        public PaymentIntent? CreatePaymentIntent(int orderAmount, string currency)
        {
            return _paymentIntentService.Create(new PaymentIntentCreateOptions
            {
                Amount = orderAmount,
                Currency = currency,
                // In the latest version of the API, specifying the `automatic_payment_methods` parameter is optional because Stripe enables its functionality by default.
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
            });
        }
        public PaymentIntent? GetPaymentIntent(string paymentIntentId)
        {
            return _paymentIntentService.Get(paymentIntentId);
        }
        public PaymentIntent? GetPaymentIntent(string paymentIntentId, PaymentIntentGetOptions options)
        {
            return _paymentIntentService.Get(paymentIntentId, options);
        }


        public PaymentIntentGetOptions CustomerDetailsPaymentIntentOptions()
        {
            var options = new PaymentIntentGetOptions();
            options.AddExpand("customer");
            options.AddExpand("line_items");
            return options;
        }
        public SessionGetOptions CustomerDetailsSessionDetailsOptions()
        {
            var options = new SessionGetOptions();
            options.AddExpand("customer");
            options.AddExpand("line_items");
            return options;
        }
    }
}
