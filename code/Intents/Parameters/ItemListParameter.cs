using Sitecore.Data;
using Sitecore.Data.Items;
using SitecoreCognitiveServices.Feature.OleChat.Services;
using SitecoreCognitiveServices.Feature.OleChat.Statics;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Enums;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Factories;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;
using SitecoreCognitiveServices.Foundation.SCSDK.Wrappers;
using System.Collections.Generic;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents.Parameters
{
    public class ItemListParameter : IRequiredParameter
    {
        #region Constructor

        public string ParamName { get; set; }
        
        public Dictionary<string, string> Parameters { get; set; }

        public ISitecoreDataWrapper DataWrapper { get; set; }
        public IIntentInputFactory IntentInputFactory { get; set; }
        public IParameterResultFactory ResultFactory { get; set; }
        public ISearchService Searcher { get; set; }

        public static string DataKey = "Item List Data";
        public static string TemplateNameKey = "Template Name";
        public static string FieldNameKey = "Field Name";
        public static string FieldValueKey = "Field Value";
        public static string FolderPathKey = "Folder Path";
        public static string ItemNameKey = "Item Name";

        public ItemListParameter(
            string paramName,
            Dictionary<string, string> parameters,
            ISitecoreDataWrapper dataWrapper,
            IIntentInputFactory inputFactory,
            IParameterResultFactory resultFactory,
            ISearchService searcher) 
        {
            ParamName = paramName;
            Parameters = parameters;
            DataWrapper = dataWrapper;
            IntentInputFactory = inputFactory;
            ResultFactory = resultFactory;
            Searcher = searcher;
        }

        #endregion

        public IParameterResult GetParameter(string paramValue, IConversationContext context)
        {
            var conversation = context.GetCurrentConversation();
            var hasParamValue = string.IsNullOrWhiteSpace(paramValue);
            var dataExists = conversation.Data.ContainsKey(DataKey);

            //find or setup temp storage
            var data = dataExists
                ? (Dictionary<string, string>)conversation.Data[DataKey].Value
                : new Dictionary<string, string>();

            if (!dataExists)
                conversation.Data[DataKey] = new ParameterData { Value = data };
            
            var fieldList = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(TemplateNameKey, Translator.Text("Chat.Parameters.ItemList.TemplateNameRequest")),
                new KeyValuePair<string, string>(FieldNameKey, Translator.Text("Chat.Parameters.ItemList.FieldNameRequest")),
                new KeyValuePair<string, string>(FieldValueKey, Translator.Text("Chat.Parameters.ItemList.FieldValueRequest")),
                new KeyValuePair<string, string>(FolderPathKey, Translator.Text("Chat.Parameters.ItemList.FolderPathRequest")),
                new KeyValuePair<string, string>(ItemNameKey, Translator.Text("Chat.Parameters.ItemList.ItemNameRequest"))
            };

            for (int i = 0; i < fieldList.Count; i++)
            {
                var param = fieldList[i];
                if (data.ContainsKey(param.Key))
                    continue;

                //needs validation also need to have different inputs for search or text 
                //or list might need to be individual parameters
                //var isValid = (paramValue);

                if (string.IsNullOrWhiteSpace(paramValue))
                    return ResultFactory.GetFailure("That's not a valid value.");

                data[param.Key] = paramValue;

                //if no next param them return 
                var j = i + 1;
                if (j == fieldList.Count)
                    continue;

                var nextKey = fieldList[j];
                
                return ResultFactory.GetFailure(nextKey.Value);
            }

            //remove temp storage
            conversation.Data.Remove(DataKey);

            //do search
            var searchParameters = new Dictionary<string, string>
            {
                { Constants.SearchParameters.TemplateName, data[TemplateNameKey] },
                { Constants.SearchParameters.FieldName, data[FieldNameKey] },
                { Constants.SearchParameters.FieldValue, data[FieldValueKey] },
                { Constants.SearchParameters.FilterPath, data[FolderPathKey] },
                { Constants.SearchParameters.ItemName, data[ItemNameKey] }
            };

            var results = Searcher.GetResults(context.Parameters.Database, context.Parameters.Language, "", searchParameters, 0, -1);
            
            return ResultFactory.GetSuccess(string.Join(", ", data.Values), results);
        }

        public IntentInput GetInput(ItemContextParameters parameters, IConversation conversation)
        {
            return IntentInputFactory.Create(IntentInputType.ItemSearch, "", Parameters);
        }
    }
}