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
using System.Web.UI.WebControls;

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
            var hasParamValue = !string.IsNullOrWhiteSpace(paramValue);
            var dataExists = conversation.Data.ContainsKey(ParamName);
            if (!dataExists)
                conversation.Data[ParamName] = new ParameterData { Value = new ListItem("", ""), IsIncomplete = true };

            var data = (ListItem)conversation.Data[ParamName].Value;
            
            if (string.IsNullOrWhiteSpace(data.Text))
            {
                if (hasParamValue)
                {
                    hasParamValue = false;
                    data.Text = paramValue;
                }   
                else
                {
                    return ResultFactory.GetFailure(FirstParamMessage);
                }
            }
            
            //if has first response then ask for second request 
            if (string.IsNullOrWhiteSpace(data.Value))
            {
                if (hasParamValue)
                {
                    data.Value = paramValue;
                    conversation.Data[ParamName].IsIncomplete = false;
                }   
                else
                {
                    return ResultFactory.GetFailure(SecondParamMessage);
                }
            }
            
            return ResultFactory.GetSuccess($"{data.Text} : {data.Value}", data);
        }

        public IntentInput GetInput(ItemContextParameters parameters, IConversation conversation)
        {
            return IntentInputFactory.Create(IntentInputType.Text);
        }
    }
}