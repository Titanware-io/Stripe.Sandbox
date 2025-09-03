using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Accelerate.Foundations.Common.Helpers
{
    public sealed class SubstituteNullWithEmptyStringContractResolver : CamelCasePropertyNamesContractResolver
    {
        private sealed class NullToEmptyStringValueProvider : IValueProvider
        {
            private readonly IValueProvider Provider;

            private readonly NullValueHandling? NullHandling;

            public NullToEmptyStringValueProvider(IValueProvider provider, NullValueHandling? nullValueHandling)
            {
                Provider = provider ?? throw new ArgumentNullException("provider");
                NullHandling = nullValueHandling;
            }

            public object GetValue(object target)
            {
                if (NullHandling.HasValue && NullHandling.Value == NullValueHandling.Ignore && Provider.GetValue(target) == null)
                {
                    return null;
                }

                return Provider.GetValue(target) ?? "";
            }

            public void SetValue(object target, object value)
            {
                Provider.SetValue(target, value);
            }
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty jsonProperty = base.CreateProperty(member, memberSerialization);
            if (jsonProperty.PropertyType == typeof(string))
            {
                jsonProperty.ValueProvider = new NullToEmptyStringValueProvider(jsonProperty.ValueProvider, jsonProperty.NullValueHandling);
            }

            return jsonProperty;
        }
    }
}
