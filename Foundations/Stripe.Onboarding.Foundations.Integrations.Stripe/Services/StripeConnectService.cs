using Microsoft.Extensions.Options;
using Stripe.Checkout;
using Stripe.FinancialConnections;
using Stripe.Onboarding.Foundations.Integrations.Stripe.Models.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stripe.Onboarding.Foundations.Integrations.Stripe.Services
{
    public class StripeConnectService : IStripeConnectService
    {
        AccountSessionService _accountSessionService { get; set; }
        AccountCreateOptions _options { get; set; }
        StripeConfig _config { get; set; }
        IStripeClient _stripeClient { get; set; }
        AccountService _accountService { get; set; }
        AccountLinkService _accountLinkService { get; set; }
        AccountLoginLinkService _accountLoginLinkService { get; set; }

        ChargeService _chargeService { get; set; }
        public StripeConnectService(IOptions<StripeConfig> options)
        {
            _config = options.Value;
             
            _stripeClient = new StripeClient(_config.SecretKey);
            
            _accountService = new AccountService(_stripeClient);
            _accountLinkService = new AccountLinkService(_stripeClient);
            _accountLoginLinkService = new AccountLoginLinkService(_stripeClient);
            _accountSessionService = new AccountSessionService(_stripeClient);
        }
        public StripeConfig Config
        {
            get
            {
                return _config;
            }
        }

        public async Task<Account> GetAccount(string connectedAccountId)
        {
            return await _accountService.GetAsync(connectedAccountId);
        }
        public async Task<LoginLink> GetLoginLink(string connectedAccountId)
        {
            return await _accountLoginLinkService.CreateAsync(connectedAccountId);
        }

        public bool IsAccountOnboarded(Account account)
        {
            return account.PayoutsEnabled && account.DetailsSubmitted && account.ChargesEnabled;
        }

        public async Task<StripeList<Account>> GetAccounts(AccountListOptions? listOptions = null, RequestOptions? options = null)
        {
            return await _accountService.ListAsync(listOptions);
        }

        public async Task<Account> CreateAccountHostedFromOptions()
        {
            var accountOptions = new AccountCreateOptions
            {
                Controller = new AccountControllerOptions
                {
                    StripeDashboard = new AccountControllerStripeDashboardOptions { Type = "express" },
                    Fees = new AccountControllerFeesOptions { Payer = "application" },
                    Losses = new AccountControllerLossesOptions { Payments = "application" },
                    RequirementCollection = "application",
                },
                Capabilities = new AccountCapabilitiesOptions
                {
                    Transfers = new AccountCapabilitiesTransfersOptions { Requested = true },
                },
                Country = "NZ",
            }; 
            return await _accountService.CreateAsync(accountOptions);
        }
        public async Task<Account> CreateAccountEmbeddedFromOptions()
        {
            var accountOptions = new AccountCreateOptions
            {
                Controller = new AccountControllerOptions
                {
                    Fees = new AccountControllerFeesOptions { Payer = "application" },
                    Losses = new AccountControllerLossesOptions { Payments = "application" },
                    StripeDashboard = new AccountControllerStripeDashboardOptions
                    {
                        Type = "express",
                    },
                },
                Country = "NZ",
            };
            return await _accountService.CreateAsync(accountOptions);
        }
        public async Task<Account> CreateAccountApiFromOptions()
        {
            var accountOptions = new AccountCreateOptions
            {
                Controller = new AccountControllerOptions
                {
                    Losses = new AccountControllerLossesOptions { Payments = "application" },
                    Fees = new AccountControllerFeesOptions { Payer = "application" },
                    StripeDashboard = new AccountControllerStripeDashboardOptions { Type = "none" },
                    RequirementCollection = "application",
                },
                Capabilities = new AccountCapabilitiesOptions
                {
                    CardPayments = new AccountCapabilitiesCardPaymentsOptions { Requested = true },
                    Transfers = new AccountCapabilitiesTransfersOptions { Requested = true },
                },
                BusinessType = "individual",
                Country = "NZ",
            };
            return await _accountService.CreateAsync(accountOptions);
        }
        public async Task<AccountLink> LinkAccountFromOptions(string connectedAccountId, string returnUrl, string refreshUrl)
        {
            return await _accountLinkService.CreateAsync(
                new AccountLinkCreateOptions
                {
                    Account = connectedAccountId,
                    ReturnUrl = returnUrl,
                    RefreshUrl = refreshUrl,
                    Type = "account_onboarding",
                }
            );
        }
        public async Task<AccountSession> CreateAccountSession(string accountId)
        {
            var options = new AccountSessionCreateOptions
            {
                Account = accountId,
                Components = new AccountSessionComponentsOptions
                {
                    AccountOnboarding = new AccountSessionComponentsAccountOnboardingOptions
                    {
                        Enabled = true,
                    },
                    // Management
                    AccountManagement = new AccountSessionComponentsAccountManagementOptions
                    {
                        Enabled = true,
                        Features = new AccountSessionComponentsAccountManagementFeaturesOptions
                        {
                            ExternalAccountCollection = true,
                        },
                    },
                    // payments
                    Payments = new AccountSessionComponentsPaymentsOptions
                    {
                        Enabled = true,
                        Features = new AccountSessionComponentsPaymentsFeaturesOptions
                        {
                            RefundManagement = true,
                            DisputeManagement = true,
                            CapturePayments = true,
                            DestinationOnBehalfOfChargeManagement = false,
                        },
                    },
                    // Payouts
                    Payouts = new AccountSessionComponentsPayoutsOptions
                    {
                        Enabled = true,
                        Features = new AccountSessionComponentsPayoutsFeaturesOptions
                        {
                            InstantPayouts = true,
                            StandardPayouts = true,
                            EditPayoutSchedule = true,
                            ExternalAccountCollection = true,
                        },
                    },
                    // Tax
                    TaxSettings = new AccountSessionComponentsTaxSettingsOptions { Enabled = true },
                    Documents = new AccountSessionComponentsDocumentsOptions { Enabled = true },
                },
            };
            return await _accountSessionService.CreateAsync(options);
        }
        public async Task<AccountSession> CreateDashboardAccountDetailsSession(string accountId)
        {
            var options = new AccountSessionCreateOptions
            {
                Account = accountId,
                Components = new AccountSessionComponentsOptions
                {
                    AccountManagement = new AccountSessionComponentsAccountManagementOptions
                    {
                        Enabled = true,
                        Features = new AccountSessionComponentsAccountManagementFeaturesOptions
                        {
                            ExternalAccountCollection = true,
                        },
                    },
                },
            };
            return await _accountSessionService.CreateAsync(options);
        }
        public async Task<AccountSession> CreateDashboardAccountPaymentsSession(string accountId)
        {
            var options = new AccountSessionCreateOptions
            {
                Account = accountId,
                Components = new AccountSessionComponentsOptions
                {
                    Payments = new AccountSessionComponentsPaymentsOptions
                    {
                        Enabled = true,
                        Features = new AccountSessionComponentsPaymentsFeaturesOptions
                        {
                            RefundManagement = true,
                            DisputeManagement = true,
                            CapturePayments = true,
                            DestinationOnBehalfOfChargeManagement = false,
                        },
                    },
                },
            };
            return await _accountSessionService.CreateAsync(options);
        }
        public async Task<AccountSession> CreateDashboardAccountPayoutsSession(string accountId)
        {
            var options = new AccountSessionCreateOptions
            {
                Account = accountId,
                Components = new AccountSessionComponentsOptions
                {
                    Payouts = new AccountSessionComponentsPayoutsOptions
                    {
                        Enabled = true,
                        Features = new AccountSessionComponentsPayoutsFeaturesOptions
                        {
                            InstantPayouts = true,
                            StandardPayouts = true,
                            EditPayoutSchedule = true,
                            ExternalAccountCollection = true,
                        },
                    },
                },
            };
            return await _accountSessionService.CreateAsync(options);
        }
        public async Task<AccountSession> CreateDashboardAccountTaxSession(string accountId)
        {
            var options = new AccountSessionCreateOptions
            {
                Account = accountId,
                Components = new AccountSessionComponentsOptions
                {
                    TaxSettings = new AccountSessionComponentsTaxSettingsOptions { Enabled = true },
                },
            };
            return await _accountSessionService.CreateAsync(options);
        }
        public async Task<AccountSession> CreateDashboardAccountDocumentsSession(string accountId)
        {
            var options = new AccountSessionCreateOptions
            {
                Account = accountId,
                Components = new AccountSessionComponentsOptions {
                    Documents = new AccountSessionComponentsDocumentsOptions { Enabled = true },
                },
            };
            return await _accountSessionService.CreateAsync(options);
        }
    }
}
