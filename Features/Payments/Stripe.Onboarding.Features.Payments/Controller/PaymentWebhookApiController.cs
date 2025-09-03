using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using Stripe.Onboarding.Features.Payments.Models.Data;
using Stripe.Onboarding.Foundations.Cart.Services;
using Stripe.Onboarding.Foundations.Common.Models;
using Stripe.Onboarding.Foundations.Integrations.Stripe.Services;
using System.Web.Helpers;
using stripeEvents = Stripe.EventTypes;

namespace Stripe.Onboarding.Features.Payment.Controller
{
    /// <summary>
    /// Not used on localhost/dev
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentWebhookApiController : ControllerBase
    {
        IOrderService _orderService { get; set; }
        IStripeCheckoutService _stripeService { get; set; }
        public PaymentWebhookApiController(IStripeCheckoutService stripeService, IOrderService orderService)
        {
            _stripeService = stripeService;
            _orderService = orderService;
        }


        [HttpPost("stripe")]
        public async Task<IActionResult> Stripe()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                  json,
                  Request.Headers["Stripe-Signature"],
                  _stripeService.Config.WebhookSecret
                );
                Checkout.Session session;
                
                switch (stripeEvent.Type)
                {
                    case stripeEvents.CheckoutSessionCompleted:

                        session = GetSession(stripeEvent);
                        //TODO: Include conditions when checking payment
                        UpdatePaidOrder(session);
                        // Check if the order is paid (for example, from a card payment)
                        //
                        // A delayed notification payment will have an `unpaid` status, as
                        // you're still waiting for funds to be transferred from the customer's
                        // account.
                        // Fulfill the purchase

                        // used by hosted page
                        UpdateFulfillOrder(session); 

                        break;
                    case stripeEvents.CheckoutSessionAsyncPaymentSucceeded:
                        // Fulfill the purchase
                        session = GetSession(stripeEvent);
                        UpdatePaidOrder(session);

                        break;
                    case stripeEvents.CheckoutSessionAsyncPaymentFailed:
                        session = stripeEvent.Data.Object as Checkout.Session;
                        // Send an email to the customer asking them to retry their order
                        EmailCustomerAboutFailedPayment(session);
                        
                        break;
                    case stripeEvents.PaymentIntentSucceeded:
                        var eventIntent = stripeEvent.Data.Object as Stripe.PaymentIntent;

                        break;
                }

                return Ok();
            }

            catch (StripeException e)
            {
                return BadRequest();
            }
        }
        private void UpdatePaidOrder(Checkout.Session session)
        {
            // TODO: fill me in
            // Retrieve the session. If you require line items in the response, you may include them by expanding line_items. 
            Guid orderId;
            Guid.TryParse(session.ClientReferenceId, out orderId);
            var order = _orderService.GetOrder(orderId);
            var email = session.CustomerDetails != null ? session.CustomerDetails.Email : session.CustomerEmail;
            order.CustomerBilling.Email = email ?? order.CustomerBilling.Email;
            order.PaymentReference = session.PaymentIntentId;
            order.Status = Foundations.Orders.Models.Data.OrderStatus.Paid;
            order.InvoicedAmount = (int)(session.AmountTotal / 100);
            _orderService.SaveOrder(order);
        }
        private void UpdateFulfillOrder(Checkout.Session session)
        {
            Guid orderId;
            Guid.TryParse(session.ClientReferenceId, out orderId);
            var order = _orderService.GetOrder(orderId);
            order.Status = Foundations.Orders.Models.Data.OrderStatus.Fulfilled;
            _orderService.SaveOrder(order);
        }

        private void UpdateWaitingPaymentOrder(Checkout.Session session)
        {
            Guid orderId;
            Guid.TryParse(session.ClientReferenceId, out orderId);
            var order = _orderService.GetOrder(orderId);
            order.Status = Foundations.Orders.Models.Data.OrderStatus.WaitingPayment;
            _orderService.SaveOrder(order);
        }

        private void EmailCustomerAboutFailedPayment(Checkout.Session session)
        {
            // TODO: fill me in
        }

        private Checkout.Session GetSession(Event stripeEvent)
        {
            var session = stripeEvent.Data.Object as Checkout.Session;
            var options = new SessionGetOptions();
            options.AddExpand("line_items");
            options.AddExpand("customer_details");
            //options.AddExpand("customer_email");
            options.AddExpand("invoice");

            // Retrieve the session. If you require line items in the response, you may include them by expanding line_items.
            return _stripeService.GetCheckoutSession(session.Id, options);
        } 

    }
}
