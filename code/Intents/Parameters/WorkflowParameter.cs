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
    public class WorkflowParameter : IValidationParameter
    {
        #region Constructor

        public string ParamMessage { get; set; }
        public string ItemKey { get; set; }
        public string LangKey { get; set; }

        public WorkflowParameter(
            string itemKey,
            string langKey,
            string paramMessage) 
        {
            ItemKey = itemKey;
            LangKey = langKey;
            ParamMessage = paramMessage;
        }

        #endregion

        public bool IsValid(IConversationContext context, IConversation conversation)
        {
            var rootItem = (Item)conversation.Data[ItemKey].Value;
            var language = (Language)conversation.Data[LangKey].Value;
            var langItem = rootItem.Database.GetItem(rootItem.ID, language);
            
            var worflowId = langItem.Fields[Sitecore.FieldIDs.Workflow].Value;
            var workflowItem = ID.IsID(worflowId)
                ? rootItem.Database.GetItem(new ID(worflowId))
                : null;

            var workflow = workflowItem != null 
                ? rootItem.Database.WorkflowProvider.GetWorkflow(workflowItem)
                : null;

            return workflow == null || workflow.GetState(rootItem).FinalState;
        }
    }
}