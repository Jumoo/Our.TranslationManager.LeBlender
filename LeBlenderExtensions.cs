using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Our.TranslationManager.LeBlender
{
    internal static class LeBlenderExtensions
    {
        /// <summary>
        ///  strip all non-object properties out of the value, 
        /// </summary>
        /// <remarks>
        ///     removed the "propertiesOpen" value and any future non object values
        ///     that might be added to the value (ideally these values should be in config not value).
        /// </remarks>
        public static IEnumerable<Dictionary<string, LeBlenderProperties>> GetLeblenderModel(string value)
        {
            var json = JsonConvert.DeserializeObject<IEnumerable<Dictionary<string, JToken>>>(value);

            var objectOnlyJson = json.Select(x =>
                x.Where(y => y.Value.Type == JTokenType.Object).ToDictionary(k => k.Key, v => v.Value));

            var stripped = JsonConvert.SerializeObject(objectOnlyJson);

            return JsonConvert.DeserializeObject<IEnumerable<Dictionary<string, LeBlenderProperties>>>(stripped);
        }
    }

    public class LeBlenderProperties
    {
        [JsonProperty("dataTypeGuid")]
        public String DataTypeGuid { get; set; }

        [JsonProperty("editorName")]
        public String Name { get; set; }

        [JsonProperty("editorAlias")]
        public String Alias { get; set; }

        [JsonProperty("value")]
        public object Value { get; set; }
    }


}
