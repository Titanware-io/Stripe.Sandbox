using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Onboarding.Features.Connect.Models.Views;
using Stripe.Onboarding.Features.Connect.Models.Data;
using Stripe.Onboarding.Foundations.Common.Models.Components.Form;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Options;
using static Stripe.Onboarding.Foundations.Common.Constants;
using Stripe;
using Stripe.Checkout;
using Stripe.Onboarding.Foundations.Integrations.Stripe.Services;
using Stripe.Onboarding.Foundations.Common.Controllers;
using Stripe.Onboarding.Foundations.Authentication.Services;
using Stripe.Onboarding.Foundations.Common.Models.Components;
using Stripe.FinancialConnections;
using System.Reflection;
using Accelerate.Foundations.Common.Models.Components;
using System.Dynamic;
using static System.Net.Mime.MediaTypeNames;
using Stripe.Onboarding.Foundations.Common.Models;

namespace Stripe.Onboarding.Features.Connect.Controllers
{
    public static class AccountHydrators
    {
        public static void Hydrate(this Account account, dynamic obj)
        {
            obj._Id = account.Id;
            obj.Id = new KeyValuePair<string, string>("Id", $"/Connect/Account/{account.Id}");
            obj.Name = new KeyValuePair<string, string>("Name", account.BusinessProfile.Name);
            obj.Url = new KeyValuePair<string, string>("Url", account.BusinessProfile.Url);
        }
        public static string ToCamelCase(this string str)
        {
            if (str == null || str.Length == 0) return string.Empty;
            return Char.ToLowerInvariant(str[0]) + str.Substring(1);
        }
        public static string GetApiPath(string routePath)
        {
            return $"/v1/{routePath}";
        }
    }
    public class ConnectController : BaseController
    {
        public IStripeConnectService _stripeConnectService { get; set; }
        public string Domain { get; set; }
        public ConnectController(
            IConfiguration configuration,
            IMockAuthenticationService authService,
            IStripeConnectService stripeService) : base(authService)
        {
            this.Domain = configuration[Foundations.Common.Constants.Settings.DomainKey];
            _stripeConnectService = stripeService;
        }

        #region Helpers
        public AjaxTable<dynamic> CreateAccountsTable(IEnumerable<Account> items)
        {
            return new AjaxTable<dynamic>()
            {
                CurrentPage = 0,
                Items = CreateAccountsTableRows(items),
                Headers = GetActionsTableHeader(this.CreateTransferFundsTableHeaders(), "Id"),
                Url = AccountHydrators.GetApiPath($"ConnectApi/Query?responseType=row")
            };
        }
        public List<string> CreateTransferFundsTableHeaders()
        {
            return new List<string>()
            {
                "Id", "Name", "Url"
            };
        }
        public List<dynamic> CreateAccountsTableRows(IEnumerable<Account> items)
        {
            return items.Select(CreateAccountRow)?.ToList();
        }
        public dynamic CreateAccountRow(Account account)
        {
            var obj = new ExpandoObject();
            account.Hydrate(obj);
            return obj;
        }
        public List<TableHeader> CreateTableHeaders(IDictionary<string, string> headers)
        {
            return headers.Select(x =>
            {
                return new TableHeader()
                {
                    Data = x.Key,
                    Text = x.Key,
                    Class = x.Value,
                    Value = (x.Key?.ToCamelCase().Replace(" ", string.Empty))
                };
            }).ToList();
        }
        public List<TableHeader> CreateTableHeaders(List<string> headers)
        {
            return headers.Select(x =>
            {
                return new TableHeader()
                {
                    Data = x,
                    Text = x,
                    Value = (x?.ToCamelCase().Replace(" ", string.Empty))
                };
            }).ToList();
        }
        public List<TableHeader> GetActionsTableHeader(List<string> tableHeaders, string linkHeaderName = "Name", string actionsHeaderName = "Actions")
        {
            var headers = this.CreateTableHeaders(tableHeaders);
            for (var i = 0; i < headers.Count; i++)
            {
                if (headers[i].Text == linkHeaderName)
                {
                    headers[i].Type = AclTableHeaderType.Link;
                    headers[i].Class = "px-6 py-4 font-medium text-gray-900 whitespace-nowrap dark:text-white";
                }
                if (headers[i].Text == actionsHeaderName)
                {
                    headers[i].Type = AclTableHeaderType.Buttons;
                    headers[i].Class = "px-6 py-4 font-medium text-gray-900 whitespace-nowrap dark:text-white";
                }
            }
            return headers;
        }
        #endregion

        #region Account
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            return RedirectToAction(nameof(this.Accounts));
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Accounts()
        {
            var model = this.CreateBaseContent();
            ConnectPage viewModel = new ConnectPage(model);
            var accounts = await _stripeConnectService.GetAccounts(new AccountListOptions()
            {
                Limit = 10
            });
            viewModel.AccountsTable = this.CreateAccountsTable(accounts);
            viewModel.PageTitle = "Onboard new account";
            viewModel.PageCards = new List<CardNavigationItem>()
            {
                new CardNavigationItem()
                {
                    Title = "Hosted",
                    Image = "/src/images/stripe.hosted.png",
                    Text = "Onboard using the hosted integration",
                    Href = this.Url.Action(nameof(this.AccountsHosted), "Connect", new { })
                },
                new CardNavigationItem()
                {
                    Title = "Embedded",
                    Image = "/src/images/stripe.embedded.png",
                    Text = "Onboard using the embedded integration",
                    Href = this.Url.Action(nameof(this.AccountsEmbedded), "Connect", new { })
                },
                new CardNavigationItem()
                {
                    Title = "API",
                    Image = "/src/images/stripe.api.png",
                    Text = "Onboard using the hosted API",
                    Href = this.Url.Action(nameof(this.AccountsApi), "Connect", new {  })
                },
            };
            return View(viewModel);
        }
        #region Account

        private async Task<ConnectPage> CreateConnectPage(string? accountId)
        {
            var model = this.CreateBaseContent();
            ConnectPage paymentModel = new ConnectPage(model);

            var account = await _stripeConnectService.GetAccount(accountId);

            var loginLink = await _stripeConnectService.GetLoginLink(accountId);

            paymentModel.PublicKey = _stripeConnectService.Config.PublicKey;
            paymentModel.AccountId = account.Id;

            paymentModel.PostbackUrl = $"/v1/api/connectsession/account/{account.Id}";
            paymentModel.RedirectUrl = this.Url.Action(nameof(this.AccountDetails), "Connect", new { accountId = account.Id });

            paymentModel.PageTabs = CreateAccountDetailsTabs(accountId);
            paymentModel.StripeAccountLoginUrl = loginLink?.Url;
            paymentModel.PageTitle = "Account details";
            paymentModel.Form = this.CreateAccountForm(account);
            return paymentModel;
        }
        [HttpGet("Connect/Account/{accountId}")]
        [AllowAnonymous]
        public async Task<IActionResult> AccountDetails([FromRoute] string accountId)
        {
            return View("~/Views/Connect/AccountDetails.cshtml", await this.CreateConnectPage(accountId));
        }

        [HttpGet("Connect/Account/{accountId}/Details")]
        [AllowAnonymous]
        public async Task<IActionResult> AccountDashboardDetails([FromRoute] string accountId)
        {
            var paymentModel = await this.CreateConnectPage(accountId);

            var session = await _stripeConnectService.CreateDashboardAccountDetailsSession(accountId);
            paymentModel.ClientSecret = session.ClientSecret;
            paymentModel.DashboardComponent = "account-management";
            paymentModel.PageTabs.Selected = "Details";

            return View("~/Views/Connect/AccountDetailsTab.cshtml", paymentModel);
        }

        [HttpGet("Connect/Account/{accountId}/Payments")]
        [AllowAnonymous]
        public async Task<IActionResult> AccountDashboardPayments([FromRoute] string accountId)
        {
            var paymentModel = await this.CreateConnectPage(accountId);

            var session = await _stripeConnectService.CreateDashboardAccountPaymentsSession(accountId);
            paymentModel.ClientSecret = session.ClientSecret;
            paymentModel.DashboardComponent = "payments";
            paymentModel.PageTabs.Selected = "Payments";

            return View("~/Views/Connect/AccountDetailsTab.cshtml", paymentModel);
        }
        [HttpGet("Connect/Account/{accountId}/Payouts")]
        [AllowAnonymous]
        public async Task<IActionResult> AccountDashboardPayouts([FromRoute] string accountId)
        {
            var paymentModel = await this.CreateConnectPage(accountId);

            var session = await _stripeConnectService.CreateDashboardAccountPayoutsSession(accountId);
            paymentModel.ClientSecret = session.ClientSecret;
            paymentModel.DashboardComponent = "payouts";
            paymentModel.PageTabs.Selected = "Payouts";

            return View("~/Views/Connect/AccountDetailsTab.cshtml", paymentModel);
        }
        [HttpGet("Connect/Account/{accountId}/Tax")]
        [AllowAnonymous]
        public async Task<IActionResult> AccountDashboardTax([FromRoute] string accountId)
        {
            var paymentModel = await this.CreateConnectPage(accountId);

            var session = await _stripeConnectService.CreateDashboardAccountTaxSession(accountId);
            paymentModel.ClientSecret = session.ClientSecret;
            paymentModel.DashboardComponent = "tax-settings";
            paymentModel.PageTabs.Selected = "Tax";

            return View("~/Views/Connect/AccountDetailsTab.cshtml", paymentModel);
        }
        [HttpGet("Connect/Account/{accountId}/Documents")]
        [AllowAnonymous]
        public async Task<IActionResult> AccountDashboardDocuments([FromRoute] string accountId)
        {
            var paymentModel = await this.CreateConnectPage(accountId);
            var session = await _stripeConnectService.CreateDashboardAccountDocumentsSession(accountId);
            paymentModel.ClientSecret = session.ClientSecret;
            paymentModel.DashboardComponent = "documents";
            paymentModel.PageTabs.Selected = "Documents";
            return View("~/Views/Connect/AccountDetailsTab.cshtml", paymentModel);
        }
        private NavigationGroup CreateAccountDetailsTabs(string accountId)
        {
            var model = new NavigationGroup();
            model.Items = new List<Foundations.Common.Models.NavigationItem>();
            model.Items.Add(CreateAccountTab(accountId, "Details"));
            model.Items.Add(CreateAccountTab(accountId, "Payments"));
            model.Items.Add(CreateAccountTab(accountId, "Payouts"));
            model.Items.Add(CreateAccountTab(accountId, "Tax"));
            model.Items.Add(CreateAccountTab(accountId, "Documents"));
            return model;
        }
        private Foundations.Common.Models.NavigationItem CreateAccountTab(string accountId, string tab)
        {
            return new Foundations.Common.Models.NavigationItem()
            {
                Text = tab,
                Href = GetTabUrl(accountId, tab),
            };
        }
        private string GetTabUrl(string accountId, string tab)
        {
            return $"{GetDomain()}/Connect/Account/{accountId}/{tab}";
        }

        #endregion

        #region Hosted
        [HttpGet]
        [AllowAnonymous]
        private string GetDomain()
        {
            return $"{Request.Scheme}://{Request.Host}";
        }
        private string GetReturnUrl(string type, string accountId)
        {
            return $"{GetDomain()}/Connect/Account/{type}/Return/{accountId}";
        }
        private string GetRefreshUrl(string type, string accountId)
        {
            return $"{GetDomain()}/Connect/Account/{accountId}";
        }
        public async Task<IActionResult> AccountsHosted()
        { 
            var account = await _stripeConnectService.CreateAccountHostedFromOptions();

            var returnUrl = $"{GetDomain}/Connect/Accounts/Hosted/Return/{account.Id}";
            var refreshUrl = $"{GetDomain}/Connect/Account/{account.Id}";

            var link = await _stripeConnectService.LinkAccountFromOptions(
                account.Id, 
                GetReturnUrl("Hosted", account.Id),
                GetRefreshUrl("Hosted", account.Id)
            );

            return Redirect(link.Url);
        }

        [HttpGet("Connect/Accounts/Hosted/Refresh/{accountId}")]
        [AllowAnonymous]
        public async Task<IActionResult> AccountsHostedRefresh([FromRoute] string accountId)
        { 
            var link = await _stripeConnectService.LinkAccountFromOptions(
                accountId,
                GetReturnUrl("Hosted", accountId),
                GetRefreshUrl("Hosted", accountId)
            );
            return Redirect(link.Url);
        }
        #endregion


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> AccountsEmbedded([FromQuery] string? accountId)
        {
            var model = this.CreateBaseContent();
            ConnectPage viewModel = new ConnectPage(model);
            
            Account account = string.IsNullOrEmpty(accountId)
                ? await _stripeConnectService.CreateAccountEmbeddedFromOptions()
                : await _stripeConnectService.GetAccount(accountId);
            
            var onboarded = _stripeConnectService.IsAccountOnboarded(account);
            if (onboarded)
            {
                return Redirect(this.Url.Action(nameof(this.AccountDetails), "Connect", new { accountId = account.Id }));

            }
            // Create session
            var accountSession = await _stripeConnectService.CreateAccountSession(account.Id);
            viewModel.PublicKey = _stripeConnectService.Config.PublicKey;
            viewModel.ClientSecret = accountSession.ClientSecret;
            viewModel.AccountId = account.Id;

            viewModel.PostbackUrl = $"/v1/api/connectsession/account/{account.Id}";
            viewModel.RedirectUrl = this.Url.Action(nameof(this.AccountDetails), "Connect", new { accountId = account.Id });
            //  paymentModel.PaymentForm = this.CreatePaymentForm();
            return View(viewModel);
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> AccountsApi()
        {
            var model = this.CreateBaseContent();
            ConnectPage viewModel = new ConnectPage(model);
            // Create account
            var account = await _stripeConnectService.CreateAccountEmbeddedFromOptions();
            // Create session
            var accountSession = await _stripeConnectService.CreateAccountSession(account.Id);
            viewModel.PublicKey = _stripeConnectService.Config.PublicKey;
            viewModel.ClientSecret = accountSession.ClientSecret;
            //  paymentModel.PaymentForm = this.CreatePaymentForm();
            return View(viewModel);
        }

        public Form CreateAccountForm(Account account)
        {
            var model = new Form()
            {
                Label = "Account",
                Fields = new List<FormField>()
                {
                    CreateDisabledInput("Name", account.BusinessProfile.Name),
                    CreateDisabledInput("Url", account.BusinessProfile.Url),
                }
            };
            return model;
        }
        private FormField CreateDisabledInput(string name, string value)
        {
            return new FormField()
            {
                Label = name,
                Name = name,
                FieldType = FormFieldTypes.input,
                Value = value,
                Hidden = false,
                Disabled = true,
                InvalidText = " "
            };
        }
        /*
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Pay(ConnectIntentRequest model)
        { 
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>
                {
                  new SessionLineItemOptions
                  {
                    // Provide the exact Price ID (for example, pr_1234) of the product you want to sell
                    Price = "{{PRICE_ID}}",
                    Quantity = 1,
                  },
                },
                Mode = "payment",
                SuccessUrl = this.SuccessUrl("Success"),
                CancelUrl = this.SuccessUrl("Cancel"),
            };
            var service = new SessionService();
            Session session = service.Create(options);

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }
       */
        #endregion


        #region
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Success([FromQuery] string session_id)
        {
            var model = this.CreateBaseContent();
            ConnectPage viewModel = new ConnectPage(model); 
            /*
            var session = _stripeService.GetCheckoutSession(session_id);
            paymentModel.Status = session.Status;
            paymentModel.CustomerEmail = session.CustomerEmail;
            paymentModel.PaymentForm = this.CreatePaymentForm();
            */
            return View(viewModel);
        }
        public string SuccessUrl(string message)
        {
            return this.Url.Action(
                action: nameof(Success),
                controller: "Payments",
                values: new { message },
                protocol: Request.Scheme);
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Cancel()
        {
            var model = this.CreateBaseContent();
            ConnectPage viewModel = new ConnectPage(model);
            return View(viewModel);
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Error()
        {
            var model = this.CreateBaseContent();
            ConnectPage viewModel = new ConnectPage(model);
            return View(viewModel);
        }
        #endregion
    }
}