using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Accelerate.Foundations.Common.Helpers
{
    public static class JsonSerializerHelper
    {
        private static JsonSerializerSettings settingsMinimal = new JsonSerializerSettings
        {
            ContractResolver = new SubstituteNullWithEmptyStringContractResolver(),
            Formatting = Formatting.None,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
        };
        private static JsonSerializerSettings settings = new JsonSerializerSettings
        {
            ContractResolver = new SubstituteNullWithEmptyStringContractResolver(),
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        };
        public static string SerializeMinimalObject(object obj)
        {
            return JsonConvert.SerializeObject(obj, settingsMinimal);
        }
        public static string SerializeObject(object obj)
        { 
            return JsonConvert.SerializeObject(obj, settings);
        }
        public static T DeserializeObject<T>(string obj)
        {
            return JsonConvert.DeserializeObject<T>(obj, settings);
        }
        public static T? SafelyDeserializeObject<T>(string obj)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(obj, settings);
            }
            catch (Exception ex)
            {
                return default;
            }
        }
    }
}
