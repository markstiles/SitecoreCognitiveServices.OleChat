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
    public class DatabaseParameter : IRequiredConversationParameter
    {
        #region Constructor

        public string ParamName { get; set; }
        public string ParamMessage { get; set; }

        public IOleSettings Settings { get; set; }
        public ISitecoreDataWrapper DataWrapper { get; set; }
        public IIntentInputFactory IntentInputFactory { get; set; }
        public IPublishWrapper PublishWrapper { get; set; }
        public IParameterResultFactory ResultFactory { get; set; }

        public DatabaseParameter(
            string paramName,
            IOleSettings settings,
            ISitecoreDataWrapper dataWrapper,
            IIntentInputFactory inputFactory,
            IPublishWrapper publishWrapper,
            IParameterResultFactory resultFactory)
        {
            ParamName = paramName;
            ParamMessage = Translator.Text("Chat.Parameters.DBParameterRequest");
            Settings = settings;
            DataWrapper = dataWrapper;
            IntentInputFactory = inputFactory;
            PublishWrapper = publishWrapper;
            ResultFactory = resultFactory;
        }

        #endregion

        public IParameterResult GetParameter(string paramValue, IConversationContext context, ItemContextParameters parameters, IConversation conversation)
        {
            try
            {
                return ResultFactory.GetSuccess(DataWrapper.GetDatabase(paramValue.ToLower()));
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