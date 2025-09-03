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
    public interface IStripeConnectService : IStripeService
    {
        Task<StripeList<Account>> GetAccounts(AccountListOptions? listOptions = null, RequestOptions? options = null);
        Task<Account> GetAccount(string connectedAccountId);
        Task<Account> CreateAccountHostedFromOptions();
        Task<Account> CreateAccountEmbeddedFromOptions();
        Task<Account> CreateAccountApiFromOptions();

        Task<AccountSession> CreateDashboardAccountDetailsSession(string accountId);
        Task<AccountSession> CreateDashboardAccountPaymentsSession(string accountId);
        Task<AccountSession> CreateDashboardAccountPayoutsSession(string accountId);
        Task<AccountSession> CreateDashboardAccountTaxSession(string accountId);
        Task<AccountSession> CreateDashboardAccountDocumentsSession(string accountId);

        Task<LoginLink> GetLoginLink(string connectedAccountId);
        Task<AccountSession> CreateAccountSession(string accountId);
        bool IsAccountOnboarded(Account account);
        Task<AccountLink> LinkAccountFromOptions(string connectedAccountId, string returnUrl, string refreshUrl);
    }
}
