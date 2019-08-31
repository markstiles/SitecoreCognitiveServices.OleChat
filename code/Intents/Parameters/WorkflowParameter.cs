using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using SitecoreCognitiveServices.Feature.OleChat.Statics;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Enums;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Factories;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;
using SitecoreCognitiveServices.Foundation.SCSDK.Wrappers;
using System.Collections.Generic;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents.Parameters
{
    public class WorkflowParameter : IConversationParameter
    {
        #region Constructor

        public string ParamName { get; set; }
        public string ParamMessage { get; set; }
        public Dictionary<string, string> Parameters { get; set; }

        public ISitecoreDataWrapper DataWrapper { get; set; }
        public IIntentInputFactory IntentInputFactory { get; set; }
        public IParameterResultFactory ResultFactory { get; set; }

        public WorkflowParameter(
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

        public IParameterResult GetParameter(string paramValue, IConversationContext context, ItemContextParameters parameters, IConversation conversation)
        {
            var rootItem = (Item)conversation.Data[ParamName].Data;
            var language = (Language)conversation.Data[ParamName].Data;
            var langItem = rootItem.Database.GetItem(rootItem.ID, language);
            
            var worflowId = langItem.Fields[Sitecore.FieldIDs.Workflow].Value;
            var workflowItem = ID.IsID(worflowId)
                ? rootItem.Database.GetItem(new ID(worflowId))
                : null;

            var workflow = workflowItem != null 
                ? rootItem.Database.WorkflowProvider.GetWorkflow(workflowItem)
                : null;

            var isFinal = workflow != null
                ? workflow.GetState(rootItem).FinalState
                : true;

            return isFinal
                ? ResultFactory.GetSuccess(rootItem.DisplayName, rootItem)
                : ResultFactory.GetFailure(ParamMessage);
        }

        public IntentInput GetInput(ItemContextParameters parameters, IConversation conversation)
        {
            return IntentInputFactory.Create(IntentInputType.ItemSearch, ParamMessage, Parameters);
        }
    }
}