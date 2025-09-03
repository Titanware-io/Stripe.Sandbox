using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Stripe.Onboarding.Foundations.Common.Models.Components.Form
{
    public enum FormFieldTypes
    {
        input, email, textarea, password,// number,
    }
    public class FormField
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public string Name { get; set; }
        public string Type => Enum.GetName(FieldType);
        [JsonIgnore]
        public FormFieldTypes FieldType { get; set; }
        public bool? Disabled { get; set; }
        public bool? Hidden { get; set; }
        public string? Icon { get; set; }
        public string? Class { get; set; }
        public string Placeholder { get; set; }
        public bool? Autocomplete { get; set; }
        public bool? AriaInvalid { get; set; }
        public object Value { get; set; }
    }
}
