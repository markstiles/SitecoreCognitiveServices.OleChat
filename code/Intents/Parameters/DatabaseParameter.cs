using SitecoreCognitiveServices.Feature.OleChat.Statics;
using SitecoreCognitiveServices.Foundation.MSSDK.Language.Models.Luis;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Enums;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Factories;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;
using SitecoreCognitiveServices.Foundation.SCSDK.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents.Parameters
{
    public class DatabaseParameter : IRequiredParameter
    {
        #region Constructor

        public string ParamName { get; set; }
        protected string ParamMessage { get; set; }
        public string GetParamMessage(IConversation conversation) => ParamMessage;

        public IOleSettings Settings { get; set; }
        public ISitecoreDataWrapper DataWrapper { get; set; }
        public IIntentInputFactory IntentInputFactory { get; set; }
        public IPublishWrapper PublishWrapper { get; set; }
        public IParameterResultFactory ResultFactory { get; set; }

        public DatabaseParameter(
            string paramName,
            string paramMessage,
            IOleSettings settings,
            ISitecoreDataWrapper dataWrapper,
            IIntentInputFactory inputFactory,
            IPublishWrapper publishWrapper,
            IParameterResultFactory resultFactory)
        {
            ParamName = paramName;
            ParamMessage = paramMessage;
            Settings = settings;
            DataWrapper = dataWrapper;
            IntentInputFactory = inputFactory;
            PublishWrapper = publishWrapper;
            ResultFactory = resultFactory;
        }

        #endregion

        public IParameterResult GetParameter(string paramValue, IConversationContext context)
        {
            try
            {
                return ResultFactory.GetSuccess(paramValue, DataWrapper.GetDatabase(paramValue.ToLower()));
            }
            catch { }

            return ResultFactory.GetFailure(Translator.Text("Chat.Parameters.DBParameterValidationError"));
        }

        public IntentInput GetInput(ItemContextParameters parameters, IConversation conversation)
        {
            var dbName = (!string.IsNullOrEmpty(parameters.Database)) ? parameters.Database : Settings.MasterDatabase;
            var publishingTargets = PublishWrapper.GetPublishingTargets(dbName);

            return IntentInputFactory.Create(IntentInputType.LinkList, publishingTargets);
        }
    }
}