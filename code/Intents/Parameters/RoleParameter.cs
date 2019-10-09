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
    public class RoleParameter : IValidationParameter
    {
        #region Constructor
        
        protected string ParamMessage { get; set; }
        public string GetParamMessage(IConversation conversation) => ParamMessage;
        public List<string> Roles { get; set; }

        public ISitecoreDataWrapper DataWrapper { get; set; }

        public RoleParameter(
            string paramMessage,
            List<string> roles,
            ISitecoreDataWrapper dataWrapper) 
        {
            Roles = roles;
            var roleNames = Roles.Select(a => GetRoleDisplayName(a));
            ParamMessage = string.Format(paramMessage, string.Join(", ", roleNames));
            DataWrapper = dataWrapper;
        }

        #endregion

        public bool IsValid(IConversationContext context)
        {
            var isInRole = Roles.Any(r => DataWrapper.ContextUser.IsInRole(r));

            return isInRole || DataWrapper.ContextUser.IsAdministrator;
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