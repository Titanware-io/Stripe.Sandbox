using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using Stripe.Onboarding.Features.Cart.Models.Data;
using Stripe.Onboarding.Features.Cart.Models.Views;
using Stripe.Onboarding.Features.Cart.Services;
using Stripe.Onboarding.Features.Payments.Models.Views;
using Stripe.Onboarding.Foundations.Authentication.Services;
using Stripe.Onboarding.Foundations.Common.Controllers;
using Stripe.Onboarding.Foundations.Common.Models;
using Stripe.Onboarding.Foundations.Common.Models.Components.Form;
using Stripe.Onboarding.Foundations.Integrations.Stripe.Services;
using System.Threading.Channels;
using System.Web;

namespace Stripe.Onboarding.Features.Cart.Controllers
{
    public class CartController : BaseController
    {
        ICartSessionService _cartSessionService { get; set; }
        public CartController(
            ICartSessionService cartSessionService,
            IMockAuthenticationService authService) : base(authService)
        {
            _cartSessionService = cartSessionService;
        }


        #region Checkout
        public async Task<IActionResult> Index()
        {
            return RedirectToAction(nameof(CartController.Checkout));
        }
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var cart = _cartSessionService.GetCart(this.GetSessionUser());
            CheckoutPage paymentModel = new CheckoutPage(this.CreateBaseContent());
            paymentModel.Cart = cart;
            paymentModel.CartForm = this.CreateCheckoutForm();
            paymentModel.CartItems = paymentModel.Cart.Items?.Count() ?? 0;
            paymentModel.CartPostbackUrl = "/api/cartsession";
            paymentModel.PaymentOptions = this.CreatePaymentOptions();
            return View(paymentModel);
        }
        private List<PaymentOption> CreatePaymentOptions()
        {
            return new List<PaymentOption>()
            {
                new PaymentOption()
                {
                    Title = "Hosted Page",
                    Description = "To quickly implement payments on a spearate url",
                    Url = this.HostedPageUrl()
                },
                new PaymentOption()
                {
                    Title = "Embedded Form",
                    Description = "Quickly implement payments but want their users to stay on their site",
                    Url = this.EmbeddedFormUrl()
                },
                new PaymentOption()
                {
                    Title = "Custom Flow",
                    Description = "For a customised payment flow, typically non-standard eCommerce sites and products",
                    Url = this.CustomFlowUrl()
                },
            };
        }
        public Form CreateCheckoutForm()
        {
            var model = new Form()
            {
                Label = "Submit",
                Fields = new List<FormField>()
                {
                    new FormField()
                    {
                        Name = "UserId",
                        FieldType = FormFieldTypes.input,
                        Hidden = true,
                        Disabled = true,
                        AriaInvalid = false,
                    },
                }
            };
            return model;
        }  
         
        #endregion
        public string HostedPageUrl()
        {
            return this.Url.Action(
                action: "HostedPage",
                controller: "Checkout",
                values: new {  },
                protocol: Request.Scheme);
        }
        public string EmbeddedFormUrl()
        {
            return this.Url.Action(
                action: "EmbeddedForm",
                controller: "Checkout",
                values: new {  },
                protocol: Request.Scheme);
        }
        public string CustomFlowUrl()
        {
            return this.Url.Action(
                action: "CustomFlow",
                controller: "Checkout",
                values: new {  },
                protocol: Request.Scheme);
        }
    }
}
