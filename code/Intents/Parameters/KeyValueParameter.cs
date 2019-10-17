using System.Linq;
using Sitecore.Data;
using Sitecore.Data.Items;
using SitecoreCognitiveServices.Feature.OleChat.Areas.SitecoreCognitiveServices.Models;
using SitecoreCognitiveServices.Feature.OleChat.Statics;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Enums;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Factories;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;
using SitecoreCognitiveServices.Foundation.SCSDK.Wrappers;
using System.Collections.Generic;
using SitecoreCognitiveServices.Feature.OleChat.Services;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents.Parameters
{
    public class KeyValueParameter : IFieldParameter
    {
        #region Constructor

        public string ParamName { get; set; }
        protected string FirstParamMessage { get; set; }
        protected string SecondParamMessage { get; set; }
        public bool IsOptional { get; set; }

        public ISitecoreDataWrapper DataWrapper { get; set; }
        public IIntentInputFactory IntentInputFactory { get; set; }
        public IParameterResultFactory ResultFactory { get; set; }
        public IProfileService ProfileService { get; set; }

        public static string DataKey = "KeyValueDataKey";
        public static string FirstParamKey = "FirstParamKey";
        public static string SecondParamKey = "SecondParamKey";

        public KeyValueParameter(
            string paramName,
            string firstParamMessage,
            string secondParamMessage,
            IIntentInputFactory inputFactory,
            IParameterResultFactory resultFactory) 
        {
            ParamName = paramName;
            FirstParamMessage = firstParamMessage;
            SecondParamMessage = secondParamMessage;
            IntentInputFactory = inputFactory;
            ResultFactory = resultFactory;
            IsOptional = false;
        }

        #endregion

        public IParameterResult GetParameter(string paramValue, IConversationContext context)
        {
            var conversation = context.GetCurrentConversation();
            var hasParamValue = string.IsNullOrWhiteSpace(paramValue);
            var dataExists = conversation.Data.ContainsKey(DataKey);

            if (!conversation.Data.ContainsKey(FirstParamKey))
            {
                if (string.IsNullOrWhiteSpace(paramValue))
                    return ResultFactory.GetFailure(FirstParamMessage);
                
                conversation.Data[FirstParamKey] = new ParameterData { Value = paramValue };
                return ResultFactory.GetFailure(SecondParamMessage);
            }
            
            //if has first response then ask for second request 
            if (!conversation.Data.ContainsKey(SecondParamKey)
                && string.IsNullOrWhiteSpace(paramValue))
                return ResultFactory.GetFailure(SecondParamMessage);

            //remove temp storage
            var key = (string)conversation.Data[FirstParamKey].Value;
            conversation.Data.Remove(FirstParamKey);
            
            return ResultFactory.GetSuccess($"{key} : {paramValue}", new KeyValuePair<string, string>(key, paramValue));
        }

        public IntentInput GetInput(ItemContextParameters parameters, IConversation conversation)
        {
            return IntentInputFactory.Create(IntentInputType.Text);
        }
    }
}