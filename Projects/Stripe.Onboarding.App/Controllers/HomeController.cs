
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Evaluation;
using Stripe.Onboarding.App.Models.View;
using Stripe.Onboarding.Features.Cart.Services;
using Stripe.Onboarding.Foundations.Authentication.Services;
using Stripe.Onboarding.Foundations.Common.Controllers;
using Stripe.Onboarding.Foundations.Common.Models;
using Stripe.Onboarding.Foundations.Products.Services;

namespace Stripe.Onboarding.App.Controllers
{
    //[Authorize]
    public class HomeController : BaseController
    {
        IProductCatalogService _productCatalogService;
        ICartSessionService _cartSessionService;
        public HomeController(IMockAuthenticationService authService, ICartSessionService cartSessionService, IProductCatalogService productCatalogService) : base(authService)
        {
            _cartSessionService = cartSessionService;
            _cartSessionService = cartSessionService;
            _productCatalogService = productCatalogService;
        }


        private BasePage CreateProductListingPage()
        {
            var cart = _cartSessionService.GetCart(this.GetSessionUser());
            var model = new ProductListingPage(this.CreateBaseContent());
            model.Cart = cart;
            model.Catalog = _productCatalogService.GetCatalog();
            model.CartPostbackUrl = "/api/cartsession/add";
            model.CartItems = cart.Items?.Count() ?? 0;
            return model;
        }
        public IActionResult Index()
        {
            return RedirectToAction("ProductListing");
        }
        public IActionResult ProductListing()
        {
            var model = this.CreateProductListingPage();
            return View(model);
        }


    }
}
