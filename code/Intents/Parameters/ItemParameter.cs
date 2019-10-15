using Sitecore.Data;
using Sitecore.Data.Items;
using SitecoreCognitiveServices.Feature.OleChat.Statics;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Enums;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Factories;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;
using SitecoreCognitiveServices.Foundation.SCSDK.Wrappers;
using System.Collections.Generic;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents.Parameters
{
    public class ItemParameter : IRequiredParameter
    {
        #region Constructor

        public string ParamName { get; set; }
        protected string ParamMessage { get; set; }
        public Dictionary<string, string> Parameters { get; set; }

        public ISitecoreDataWrapper DataWrapper { get; set; }
        public IIntentInputFactory IntentInputFactory { get; set; }
        public IParameterResultFactory ResultFactory { get; set; }

        public ItemParameter(
            string paramName,
            string paramMessage,
            Dictionary<string, string> parameters,
            ISitecoreDataWrapper dataWrapper,
            IIntentInputFactory inputFactory,
            IParameterResultFactory resultFactory) 
        {
            ParamName = paramName;
            ParamMessage = paramMessage;
            Parameters = parameters;
            DataWrapper = dataWrapper;
            IntentInputFactory = inputFactory;
            ResultFactory = resultFactory;
        }

        #endregion

        public IParameterResult GetParameter(string paramValue, IConversationContext context)
        {
            if (string.IsNullOrWhiteSpace(paramValue))
                return ResultFactory.GetFailure(ParamMessage);

            var fromDb = DataWrapper.GetDatabase(context.Parameters.Database);
            var returnItem = (paramValue.Contains("this"))
                ? fromDb.GetItem(new ID(context.Parameters.Id)) ?? fromDb.GetItem(paramValue)
                : fromDb.GetItem(paramValue);

            return (returnItem == null)
                ? ResultFactory.GetFailure(Translator.Text("Chat.Parameters.ItemParameterValidationError"))
                : ResultFactory.GetSuccess(returnItem.DisplayName, returnItem);
        }

        public IntentInput GetInput(ItemContextParameters parameters, IConversation conversation)
        {
            return IntentInputFactory.Create(IntentInputType.ItemSearch, ParamMessage, Parameters);
        }
    }
}