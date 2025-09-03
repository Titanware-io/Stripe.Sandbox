
using Stripe.Onboarding.Foundations.Authentication.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Stripe.Onboarding.Foundations.Common.Models;

namespace Stripe.Onboarding.Foundations.Common.Controllers
{
    public abstract class BaseController : Controller
    {
        IMockAuthenticationService _authService { get; set; }
        public BaseController(IMockAuthenticationService authService)
        {
            _authService = authService;
        }

        public BasePage CreateBaseContent()
        {
            var model = new BasePage();
            model.TopNavigation = new List<NavigationItem>()
            {
                new NavigationItem()
                {
                    Href = "/",
                    Text = "Products"
                },
                new NavigationItem()
                {
                    Href = "/Cart/Checkout",
                    Text = "Cart"
                },
            }; 
            return model;
        }
        public Guid GetSessionUser()
        {
            return _authService.GetSessionUser();
        }
    }
}
