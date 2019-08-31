using Sitecore.Data;
using Sitecore.Data.Items;
using SitecoreCognitiveServices.Feature.OleChat.Statics;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Enums;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Factories;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;
using SitecoreCognitiveServices.Foundation.SCSDK.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents.Parameters
{
    public class RoleParameter : IConversationParameter
    {
        #region Constructor

        public string ParamName { get; set; }
        public string ParamMessage { get; set; }
        public List<string> Roles { get; set; }

        public ISitecoreDataWrapper DataWrapper { get; set; }
        public IIntentInputFactory IntentInputFactory { get; set; }
        public IParameterResultFactory ResultFactory { get; set; }

        public RoleParameter(
            string paramName,
            string paramMessage,
            List<string> roles,
            ISitecoreDataWrapper dataWrapper,
            IIntentInputFactory inputFactory,
            IParameterResultFactory resultFactory) 
        {
            ParamName = paramName;
            Roles = roles;
            var roleNames = Roles.Select(a => GetRoleDisplayName(a));
            ParamMessage = string.Format(paramMessage, string.Join(", ", roleNames));
            DataWrapper = dataWrapper;
            IntentInputFactory = inputFactory;
            ResultFactory = resultFactory;
        }

        #endregion

        public IParameterResult GetParameter(string paramValue, IConversationContext context, ItemContextParameters parameters, IConversation conversation)
        {
            var isInRole = Roles.Any(r => DataWrapper.ContextUser.IsInRole(r));
            
            return isInRole || DataWrapper.ContextUser.IsAdministrator
                ? ResultFactory.GetSuccess(paramValue, isInRole)
                : ResultFactory.GetFailure(ParamMessage);
        }

        public IntentInput GetInput(ItemContextParameters parameters, IConversation conversation)
        {
            conversation.IsEnded = true;

            return IntentInputFactory.Create(IntentInputType.None, ParamMessage);
        }

        protected string GetRoleDisplayName(string roleName)
        {
            var splitter = @"\";
            if (!roleName.Contains(splitter))
                return roleName;

            var parts = roleName.Split(new[] { splitter }, StringSplitOptions.RemoveEmptyEntries);

            return parts.Length > 1
                ? parts[1]
                : parts[0];
        }
    }
}