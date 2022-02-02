using Jumoo.TranslationManager.Core.Models;
using Jumoo.TranslationManager.LinkUpdater.LinkMappers;
using Jumoo.TranslationManager.LinkUpdater;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Umbraco.Core.Services;

using Umbraco.Core;

namespace Our.TranslationManager.LeBlender
{
    public class LeBlenderMapper : LinkMapperBase, ILinkMapper
    {
        private readonly IDataTypeService dataTypeService;

        public LeBlenderMapper(IDataTypeService dataTypeService, LinkResolver linkResolver)
            : base(linkResolver)
        {
            this.dataTypeService = dataTypeService;
        }

        public string Name => "Leblender Link Mapper";
        public string[] Editors => new string[] {
            "Umbraco.Grid.LeBlender",
            "Umbraco.Grid.LeBlendereditor",
        };

        public object UpdateLinkValues(TranslationSet set, int targetSiteId, object sourceValue, object targetValue)
        {
            if (set == null || sourceValue == null || targetValue == null) return targetValue;

            var target = targetValue.TryConvertTo<string>();
            if (!target.Success) return targetValue;

            var source = sourceValue.TryConvertTo<string>();
            if (!source.Success) return targetValue;

            if (!target.Result.DetectIsJson() || !source.Result.DetectIsJson()) return targetValue;

            var sourceJson = LeBlenderExtensions.GetLeblenderModel(source.Result).ToList();
            if (sourceJson == null | !sourceJson.Any()) return targetValue;

            var targetJson = LeBlenderExtensions.GetLeblenderModel(target.Result).ToList();
            if (targetJson == null | !targetJson.Any()) return targetValue;

            if (sourceJson.Count() != targetJson.Count()) return targetValue;

            for (int x = 0; x < targetJson.Count(); x++)
            {
                var targetItem = targetJson[x];
                var sourceItem = sourceJson[x];

                foreach (var targetProperty in targetItem)
                {
                    if (!sourceItem.ContainsKey(targetProperty.Key)) continue;

                    var sourceProperty = sourceItem[targetProperty.Key];

                    var editorAlias = GetEdtiorAlias(targetProperty.Value.DataTypeGuid);
                    if (editorAlias == string.Empty) continue;

                    var mapper = LinkMapperFactory.GetMapper(editorAlias);
                    if (mapper != null)
                    {
                        var sourceVal = sourceProperty.Value.TryConvertTo<string>();
                        var targetVal = targetProperty.Value.Value.TryConvertTo<string>();

                        if (sourceVal.Success && targetVal.Success)
                        {
                            var value = mapper.UpdateLinkValues(set, targetSiteId, sourceVal.Result, targetVal.Result);
                            var text = value.TryConvertTo<string>();
                            if (text.Success)
                            {
                                if (text.Result.DetectIsJson())
                                {
                                    targetProperty.Value.Value = JToken.Parse(text.Result);
                                }
                                else
                                {
                                    targetProperty.Value.Value = text.Result;
                                }
                            }
                        }
                    }
                }
            }

            return JsonConvert.SerializeObject(targetJson, Formatting.Indented);
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
