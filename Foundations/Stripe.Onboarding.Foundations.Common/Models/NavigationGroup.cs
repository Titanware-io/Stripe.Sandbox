using Newtonsoft.Json;

namespace Stripe.Onboarding.Foundations.Common.Models
{
    public class NavigationGroup
    {
        public string Selected { get; set; }
        public string Title { get; set; }

        public string Subtitle { get; set; }

        [JsonProperty("img", NullValueHandling = NullValueHandling.Ignore)]
        public string Image { get; set; }

        [JsonProperty("items")]
        public List<NavigationItem> Items { get; set; }

    }
}
