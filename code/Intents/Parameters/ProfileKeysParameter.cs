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
    public class ProfileKeysParameter : IRequiredParameter
    {
        #region Constructor

        public string ParamName { get; set; }
        protected string ParamMessage { get; set; }
        public string GetParamMessage(IConversation conversation)
        {
            var profileItem = (Item)conversation.Data[ProfileItemKey].Value;
            var keys = ProfileService.GetProfileKeys(profileItem);
            if (keys.Any()) {
                var firstKey = keys.First();
                var firstKeyName = ProfileService.GetProfileName(firstKey);
                var minValue = ProfileService.GetMinValue(firstKey);
                var maxValue = ProfileService.GetMaxValue(firstKey);

                return string.Format(ParamMessage, firstKeyName, minValue, maxValue);
            }

            return "There are no keys on this profile";
        }
        public Dictionary<string, string> Parameters { get; set; }

        public ISitecoreDataWrapper DataWrapper { get; set; }
        public IIntentInputFactory IntentInputFactory { get; set; }
        public IParameterResultFactory ResultFactory { get; set; }
        public IProfileService ProfileService { get; set; }

        public static string DataKey = "ProfileKeysDataKey";
        public string ProfileItemKey { get; set; }

        public ProfileKeysParameter(
            string profileItemKey,
            string paramName,
            string paramMessage,
            IIntentInputFactory inputFactory,
            IParameterResultFactory resultFactory,
            IProfileService profileService) 
        {
            ProfileItemKey = profileItemKey;
            ParamName = paramName;
            ParamMessage = paramMessage;
            IntentInputFactory = inputFactory;
            ResultFactory = resultFactory;
            ProfileService = profileService;
        }

        #endregion

        public IParameterResult GetParameter(string paramValue, IConversationContext context)
        {
            var conversation = context.GetCurrentConversation();
            var profileItem = (Item)conversation.Data[ProfileItemKey].Value;

            //find or setup temp storage
            var dataExists = conversation.Data.ContainsKey(DataKey);
            var data = dataExists
                ? (Dictionary<string, string>)conversation.Data[DataKey].Value
                : new Dictionary<string, string>();

            if(!dataExists)
                conversation.Data[DataKey] = new ParameterData { Value = data };

            var keys = ProfileService.GetProfileKeys(profileItem);
            for (int i = 0; i < keys.Count; i++)
            {
                var k = keys[i];
                var keyName = k.Fields["Name"].Value;
                if (data.ContainsKey(keyName))
                    continue;

                int outInt = 0;
                if (!int.TryParse(paramValue, out outInt))
                    return ResultFactory.GetFailure(Translator.Text("Chat.Parameters.ProfileKeysValidationError"));

                data[keyName] = paramValue;
                
                //if no next param them return 
                var j = i + 1;
                if (j == keys.Count)
                    continue;

                var nextKey = keys[j];
                var nextKeyName = ProfileService.GetProfileName(nextKey);
                var minValue = ProfileService.GetMinValue(nextKey);
                var maxValue = ProfileService.GetMaxValue(nextKey);

                return ResultFactory.GetFailure(string.Format(ParamMessage, nextKeyName, minValue, maxValue));
            }

            //remove temp storage
            conversation.Data.Remove(DataKey);

            return ResultFactory.GetSuccess(string.Join(", ", data.Values), data);
        }

        public IntentInput GetInput(ItemContextParameters parameters, IConversation conversation)
        {
            return IntentInputFactory.Create(IntentInputType.Text, GetParamMessage(conversation));
        }
    }
}