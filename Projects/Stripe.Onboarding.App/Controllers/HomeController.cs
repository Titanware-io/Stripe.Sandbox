
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

        private BasePage CreateBasePage()
        {
            var model = this.CreateBaseContent();
            
            return model;
        }
        private BasePage CreateProductsPage()
        {
            var cart = _cartSessionService.GetCart(this.GetSessionUser());
            var model = CreateBasePage();
            var viewModel = new ProductListingPage(model);
            viewModel.Cart = cart;
            viewModel.Catalog = _productCatalogService.GetCatalog();
            viewModel.CartPostbackUrl = "/api/cartsession/add";
            viewModel.CartItems = cart.Items?.Count() ?? 0;
            return viewModel;
        }
        private BasePage CreateConnectPage()
        {
            var cart = _cartSessionService.GetCart(this.GetSessionUser());
            var model = CreateBasePage();
            var viewModel = new ProductListingPage(this.CreateBaseContent());
            viewModel.Cart = cart;
            viewModel.Catalog = _productCatalogService.GetCatalog();
            viewModel.CartPostbackUrl = "/api/cartsession/add";
            viewModel.CartItems = cart.Items?.Count() ?? 0;
            return viewModel;
        }
        public IActionResult Index()
        {
            return RedirectToAction("Products");
        }

        [HttpGet("Products")]
        public IActionResult Products()
        {
            var model = this.CreateProductsPage();
            return View(model);
        }
        /*
        [Route("Connect")]
        public IActionResult Connect()
        {
            var model = this.CreateConnectPage();
            return View(model);
        }
        */

    }
}
