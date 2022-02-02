using Jumoo.TranslationManager.Core;
using Jumoo.TranslationManager.Core.Models;
using Jumoo.TranslationManager.Core.ValueMappers;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Linq;

using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;

namespace Our.TranslationManager.LeBlender
{
    public class LeBlenderValueMapper : BaseValueMapper, IValueMapper
    {
        public string Name => "Leblender Value Mapper";
        public override string[] Editors => new string[] {
            "Umbraco.Grid.LeBlender",
            "Umbraco.Grid.LeBlendereditor",
        };

        public LeBlenderValueMapper(IContentService contentService, IDataTypeService dataTypeService, IContentTypeService contentTypeService, ILogger logger)
            : base(contentService, dataTypeService, contentTypeService, logger)
        { }

        public TranslationValue GetSourceValue(string displayName, string propertyTypeAlias, object value, CultureInfoView culture)
        {
            var attempt = value.TryConvertTo<string>();
            if (!attempt || string.IsNullOrWhiteSpace(attempt.Result)) return null;

            logger.Debug<LeBlenderValueMapper>("{alias} {value}", propertyTypeAlias, attempt.Result);

            var jsonValue = LeBlenderExtensions.GetLeblenderModel(attempt.Result);
            if (jsonValue == null || !jsonValue.Any()) return null;

            logger.Debug<LeBlenderValueMapper>("{alias} {value}", propertyTypeAlias, JsonConvert.SerializeObject(jsonValue));

            var translationValue = new TranslationValue(displayName, propertyTypeAlias);

            var count = 0;

            foreach (var item in jsonValue)
            {
                count++;
                var itemValue = new TranslationValue(displayName, propertyTypeAlias, count);

                var position = 0;
                foreach (var property in item)
                {
                    position++;
                    logger.Debug<LeBlenderValueMapper>("Property: [{key}] {alias} {value} {guid}",
                        property.Key, property.Value.Alias, property.Value.Value, property.Value.DataTypeGuid);

                    var editorAlias = GetEdtiorAlias(property.Value.DataTypeGuid);
                    if (editorAlias == string.Empty)
                        continue;

                    string propertyDisplayName = string.Format("{0} {1}", displayName, property.Value.Name);

                    var innerValue = ValueMapperFactory.GetMapperSource(editorAlias, propertyDisplayName, property.Value.Value, culture);
                    if (innerValue != null)
                    {
                        innerValue.SortOrder = position;
                        itemValue.InnerValues.Add(property.Value.Alias, innerValue);
                    }
                }

                if (itemValue.HasChildValues())
                {
                    translationValue.InnerValues.Add(count.ToString(), itemValue);
                }
            }

            if (translationValue.HasChildValues())
                return translationValue;

            return null;
        }

        public object GetTargetValue(string propertyTypeAlias, object sourceValue, TranslationValue values, CultureInfoView sourceCulture, CultureInfoView targetCulture)
        {
            logger.Debug<LeBlenderValueMapper>("{alias}", propertyTypeAlias);

            var attempt = sourceValue.TryConvertTo<string>();
            if (!attempt) return null;

            var jsonValue = LeBlenderExtensions.GetLeblenderModel(attempt.Result);
            if (jsonValue == null || !jsonValue.Any()) return null;

            logger.Debug<LeBlenderValueMapper>("Found {count} items", jsonValue.Count());

            var count = 0;

            foreach (var item in jsonValue)
            {

                logger.Debug<LeBlenderValueMapper>("Getting values for {item}", item.ToString());
                count++;

                var itemValue = values.GetInnerValue(count.ToString());
                if (itemValue == null)
                    continue;

                foreach (var property in item)
                {
                    var propertyValue = itemValue.GetInnerValue(property.Value.Alias);
                    if (propertyValue == null)
                        continue;

                    var editorAlias = GetEdtiorAlias(property.Value.DataTypeGuid);
                    if (editorAlias == string.Empty)
                        continue;



                    var value = ValueMapperFactory.GetMapperTarget(editorAlias, property.Value.Value, propertyValue, sourceCulture, targetCulture);
                    var valueText = value.TryConvertTo<string>();
                    if (!valueText.Success || valueText.Result == null)
                        continue;

                    if (valueText.Result.DetectIsJson())
                    {
                        property.Value.Value = JToken.Parse(valueText.Result);
                    }
                    else
                    {
                        property.Value.Value = valueText.Result;
                    }
                }
            }

            logger.Debug<LeBlenderValueMapper>("LeBlender Value: {json}", JsonConvert.SerializeObject(jsonValue));

            return JsonConvert.SerializeObject(jsonValue);
        }

        private string GetEdtiorAlias(string dtdGuidString)
        {
            if (string.IsNullOrWhiteSpace(dtdGuidString))
                return string.Empty;

            var propEditor = string.Empty;
            if (Guid.TryParse(dtdGuidString, out Guid dtdGuid))
            {
                var dtd = dataTypeService.GetDataType(dtdGuid);
                if (dtd != null)
                {
                    return dtd.EditorAlias;
                }
            }

            return string.Empty;
        }
    }
}
