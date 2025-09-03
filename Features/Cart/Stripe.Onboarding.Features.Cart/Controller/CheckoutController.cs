using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe.Checkout;
using Stripe.Climate;
using Stripe.Onboarding.Features.Cart.Helpers;
using Stripe.Onboarding.Features.Cart.Hydrators;
using Stripe.Onboarding.Features.Cart.Models.Data;
using Stripe.Onboarding.Features.Cart.Models.Views;
using Stripe.Onboarding.Features.Cart.Services;
using Stripe.Onboarding.Foundations.Authentication.Services;
using Stripe.Onboarding.Foundations.Cart.Services;
using Stripe.Onboarding.Foundations.Common.Controllers;
using Stripe.Onboarding.Foundations.Common.Models;
using Stripe.Onboarding.Foundations.Common.Models.Components.Form;
using Stripe.Onboarding.Foundations.Integrations.Stripe.Services;
using Stripe.Onboarding.Foundations.Orders.Models.Data;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Channels;
using System.Web;

namespace Stripe.Onboarding.Features.Cart.Controllers
{
    public class CheckoutController : BaseController
    {
        IStripeCheckoutService _stripeService { get; set; }
        ICartSessionService _cartSessionService { get; set; }
        IOrderService _orderService { get; set; }
        ICheckoutViewService _checkoutViewService { get; set; }
        public CheckoutController(
            IStripeCheckoutService stripeService,
            ICartSessionService cartSessionService,
            ICheckoutViewService checkoutViewService,
            IOrderService orderService,
            IMockAuthenticationService authService) : base(authService)
        {
            _stripeService = stripeService;
            _orderService = orderService;
            _cartSessionService = cartSessionService;
            _checkoutViewService = checkoutViewService;
        }
         
        
        public Foundations.Orders.Models.Data.Order CreateOrderFromCart(Guid userId, SessionCart cart)
        {
            //Create order
            var existingOrder = _orderService.GetOrderFromCartId(cart.Id);
            if (existingOrder != null) return existingOrder;
            var orderItems = cart.Items.Select(OrderHelper.CreateOrderProductItem).ToList();
            var order = _orderService.CreateOrderFromCart(userId, cart.Id, orderItems);
            return order;
        }


        #region Customer charges
        [HttpGet]
        public async Task<IActionResult> Charges()
        {
            var paymentModel = new CheckoutPage(this.CreateBaseContent());
            var userId = this.GetSessionUser();
            var cart = _cartSessionService.GetCart(userId);
            if (cart.Items.Count() == 0)
            {
                return RedirectToAction("Checkout", "Cart");
            }
            //Create order
            var order = CreateOrderFromCart(userId, cart);
            var stripCustomerId = "cus_QMU5mGC4CaJfXy";
            paymentModel.Charges = _stripeService.GetCustomerCharges(stripCustomerId, 10);

            return View(paymentModel);
        }
        #endregion

        #region Hosted Page

        [HttpGet]
        public async Task<IActionResult> HostedPage()
        {
            var userId = this.GetSessionUser();
            var cart = _cartSessionService.GetCart(userId);
            if (cart.Items.Count() == 0)
            {
                return RedirectToAction("Checkout", "Cart");
            }
            //Create order
            var order = CreateOrderFromCart(userId, cart);
            var successUrl = this.SuccessRedirectUrl(order.Id.ToString()); 
            var cancelUrl = this.CancelUrl("Cancel");
            var options = _checkoutViewService.CreateHostedPageSessionOptions(cart, order, successUrl, cancelUrl);

            // If a subscription exists, add the ToS
            _checkoutViewService.ApplySubscriptionConsent(cart, options);
            Session session = _stripeService.CreateSession(options);
            _orderService.SaveOrder(order);
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public string SuccessRedirectUrl(string oid)
        {
            return this.GetRouteOrderUrl(nameof(SuccessRedirect), "Checkout", oid);
        }
        #endregion
        #region Embedded Form
        private CheckoutPage CreateEmbeddedFormPage(SessionCart cart)
        {
            var paymentModel = new CheckoutPage(this.CreateBaseContent());
            paymentModel.Cart = cart;
            paymentModel.CartItems = paymentModel.Cart.Items?.Count() ?? 0;
            paymentModel.PostbackUrl = this.SessionFormUrl();
            paymentModel.PublicKey = _stripeService.Config.PublicKey;
            return paymentModel;
        }
        [HttpGet]
        public async Task<IActionResult> EmbeddedForm()
        {
            var userId = this.GetSessionUser();
            var cart = _cartSessionService.GetCart(userId);
            var viewModel = this.CreateEmbeddedFormPage(cart);
            if (viewModel.CartItems == 0)
            {
                return RedirectToAction("Checkout", "Cart");
            }
            return View(viewModel);
        }

        //https://docs.stripe.com/checkout/embedded/quickstart
        [HttpPost]
        public async Task<IActionResult> CreateEmbeddedFormSession()
        {
            var userId = this.GetSessionUser();
            var cart = _cartSessionService.GetCart(userId);
            var order = CreateOrderFromCart(userId, cart);

            var returnUrl = this.ReturnUrl() + "?session_id={CHECKOUT_SESSION_ID}";
            var options = this._checkoutViewService.CreateEmbeddedFormSessionOptions(cart, order, returnUrl);
             
            // If a subscription exists, add the ToS
            _checkoutViewService.ApplySubscriptionConsent(cart, options);
            Session session = _stripeService.CreateSession(options);
            _orderService.SaveOrder(order);

            return Ok(new { clientSecret = session.RawJObject["client_secret"] });
        }
        public string ReturnUrl()
        {
            return this.GetRouteUrl(nameof(Success), "Checkout");
        }
        #endregion
        #region Custom Flow
        public string SuccessUrl()
        {
            return this.GetRouteOrderUrl(nameof(SuccessRedirect), "Checkout");
        }
        private CustomFlowPage CreateCustomFlowPage(SessionCart cart, Guid userId)
        {
            var paymentModel = new CustomFlowPage(this.CreateBaseContent());
            paymentModel.Cart = cart;
            paymentModel.CartItems = paymentModel.Cart.Items?.Count() ?? 0;
            paymentModel.ReturnUrl = this.CustomFlowUrl();
            paymentModel.PublicKey = _stripeService.Config.PublicKey;
            paymentModel.PostbackUrl = "/api/checkoutorder/paymentintent";

            paymentModel.SuccessUrl = this.SuccessUrl();
            paymentModel.OrderApiPostbackUrl = "/api/checkoutorder";

            var order = CreateOrderFromCart(userId, cart);
            _orderService.SaveOrder(order);
            paymentModel.Order = order;
            paymentModel.PaymentForm = CreateCustomPaymentForm(order);
            return paymentModel;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> CustomFlow()
        {
            var userId = this.GetSessionUser();
            var cart = _cartSessionService.GetCart(userId);
            var viewModel = this.CreateCustomFlowPage(cart, userId);

            if (viewModel.CartItems == 0)
            {
                return RedirectToAction("Checkout", "Cart");
            }


            return View(viewModel);
        }
        private AjaxForm CreateCustomPaymentForm(Foundations.Orders.Models.Data.Order order)
        {
            return new AjaxForm()
            {
                //FixTop = true,
                PostbackUrl = "/api/checkoutorder/billing",
                Type = PostbackType.POST,
                Event = "billling:submitted",
                ActionEvent = "action:post",
                Label = "Submit",
                Fields = new List<FormField>()
                {
                    new FormField()
                    {
                        Name = "OrderId",
                        FieldType = FormFieldTypes.input,
                        Placeholder = "OrderId",
                        Hidden = true,
                        Disabled = true,
                        Value = order.Id,
                        AriaInvalid = false
                    },
                    new FormField()
                    {
                        Name = "Firstname",
                        FieldType = FormFieldTypes.input,
                        Placeholder = "Firstname",
                        AriaInvalid = false
                    },
                    new FormField()
                    {
                        Name = "Lastname",
                        FieldType = FormFieldTypes.input,
                        Placeholder = "Lastname",
                        AriaInvalid = false
                    },
                    new FormField()
                    {
                        Name = "Email",
                        FieldType = FormFieldTypes.input,
                        Placeholder = "Email",
                        AriaInvalid = false
                    },
                },
            };
        }

        #endregion
        private Foundations.Orders.Models.Data.Order GetOrder(string oid)
        {
            Guid orderId;
            Guid.TryParse(oid, out orderId);
            return _orderService.GetOrder(orderId);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> SuccessRedirect()
        {
            ConfirmationPage paymentModel = new ConfirmationPage(this.CreateBaseContent());
            var request = HttpContext.Request;
            var oid = request.Query["oid"];
            var order = GetOrder(oid);
            if (order == null)
            {
                return RedirectToAction("Error");
            }
            paymentModel.OrderId = order.Id.ToString();
            paymentModel.PaymentMethodReference = order.PaymentReference;
            paymentModel.OrderAmount = order.Amount;
            paymentModel.CustomerEmail = order.CustomerEmail;
            paymentModel.InvoiceAmount = order.InvoicedAmount;
            if (order.Status == OrderStatus.Paid || order.Status == OrderStatus.Fulfilled)
            {
                this._cartSessionService.ClearCart(order.UserId);
            }
            return View("/Views/Checkout/Success.cshtml", paymentModel);
        }
        private string GetCustomerEmail(Session session)
        {
            return session.Customer != null ? session.Customer.Email : session.CustomerDetails.Email;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Success([FromQuery] string session_id)
        {
            var paymentModel = new ConfirmationPage(this.CreateBaseContent());
            var session = _stripeService.GetCheckoutSession(session_id, _stripeService.CustomerDetailsSessionDetailsOptions());
            paymentModel.PaymentMethodReference = session.PaymentIntentId;
            paymentModel.OrderId = session.PaymentIntentId;
            paymentModel.Status = session.Status;
            paymentModel.CustomerEmail = GetCustomerEmail(session);
            var order = GetOrder(session.ClientReferenceId);
            paymentModel.OrderAmount = order.Amount;
            paymentModel.InvoiceAmount = order.InvoicedAmount;
            if(order.Status == OrderStatus.Paid || order.Status == OrderStatus.Fulfilled)
            {
                this._cartSessionService.ClearCart(order.UserId);
            }
            return View(paymentModel);
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Cancel()
        {
            return View(new CheckoutPage(this.CreateBaseContent()));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Error()
        {
            return View(new CheckoutPage(this.CreateBaseContent()));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Return([FromQuery] string session_id)
        {
            return View(new CheckoutPage(this.CreateBaseContent()));
        }


        #region Urls
        public string CancelUrl(string message)
        {
            return this.GetRouteUrl(nameof(Cancel), "Checkout", message);
        }
        public string SessionFormUrl()
        {
            return this.GetRouteUrl(nameof(CreateEmbeddedFormSession), "Checkout");
        }
        public string CustomFlowUrl()
        {
            return this.GetRouteUrl(nameof(CustomFlow), "Checkout");
        }

        private string GetRouteUrl(string action, string controller, string? message = null)
        {
            return this.Url.Action(
                action: action,
                controller: controller,
                values: new { message },
                protocol: Request.Scheme);
        }
        private string GetRouteOrderUrl(string action, string controller, string? oid = null)
        {
            return this.Url.Action(
                action: action,
                controller: controller,
                values: new { oid },
                protocol: Request.Scheme);
        }
        #endregion


        #region Stripe
        // To move into a separate view service
        #endregion
    }
}
