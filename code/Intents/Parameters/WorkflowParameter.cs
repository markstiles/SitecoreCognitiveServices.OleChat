using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents.Parameters
{
    public class WorkflowParameter : IValidationParameter
    {
        #region Constructor

        protected string ParamMessage { get; set; }
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

        public string GetErrorMessage() => ParamMessage;

        public bool IsValid(IConversationContext context)
        {
            var conversation = context.GetCurrentConversation();

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