using Newtonsoft.Json;
using Stripe.Onboarding.Foundations.Common.Models;

namespace Stripe.Onboarding.Features.Connect.Models.Data
{
    public class ConnectIntentRequest
    {
        public Guid CartId { get; set; }
        public Guid OrderId { get; set; }
    }
}
