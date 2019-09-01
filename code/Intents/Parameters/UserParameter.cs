using Sitecore.Security.Accounts;
using Sitecore.Web.Authentication;
using SitecoreCognitiveServices.Feature.OleChat.Statics;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Enums;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Factories;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;
using SitecoreCognitiveServices.Foundation.SCSDK.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents.Parameters
{
    public class UserParameter : IRequiredParameter
    {
        #region Constructor

        public string ParamName { get; set; }
        public string ParamMessage { get; set; }

        public IIntentInputFactory IntentInputFactory { get; set; }
        public IParameterResultFactory ResultFactory { get; set; }
        public IAuthenticationWrapper AuthenticationWrapper { get; set; }

        public UserParameter(
            string paramName,
            IIntentInputFactory inputFactory,
            IAuthenticationWrapper authWrapper,
            IParameterResultFactory resultFactory)
        {
            ParamName = paramName;
            ParamMessage = Translator.Text("Chat.Parameters.UserParameterRequest");
            IntentInputFactory = inputFactory;
            AuthenticationWrapper = authWrapper;
            ResultFactory = resultFactory;
        }

        #endregion

        public IParameterResult GetParameter(string paramValue, IConversationContext context, ItemContextParameters parameters, IConversation conversation)
        {
            var error = Translator.Text("Chat.Parameters.UserParameterValidationError");
            var username = paramValue.Replace(" ", "");
            if (string.IsNullOrEmpty(username))
                return ResultFactory.GetFailure(error);

            string regex = @"^(\w[\w\s]*)([\\]{1})(\w[\w\s\.\@]*)$";
            Match m = Regex.Match(username, regex);
            if (string.IsNullOrEmpty(m.Value))
                return ResultFactory.GetFailure(error);

            DomainAccessGuard.Session userSession = null;
            if (Sitecore.Security.Accounts.User.Exists(username))
                userSession = AuthenticationWrapper.GetDomainAccessSessions().FirstOrDefault(
                    s => string.Equals(s.UserName, username, StringComparison.OrdinalIgnoreCase));

            return userSession == null
                ? ResultFactory.GetFailure(error)
                : ResultFactory.GetSuccess(username, userSession);
        }

        public IntentInput GetInput(ItemContextParameters parameters, IConversation conversation)
        {
            return IntentInputFactory.Create(IntentInputType.None);
        }
    }
}