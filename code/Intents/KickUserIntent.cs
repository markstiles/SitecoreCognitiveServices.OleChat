using System;
using System.Linq;
using SitecoreCognitiveServices.Foundation.SCSDK.Wrappers;
using SitecoreCognitiveServices.Foundation.MSSDK.Language.Models.Luis;
using Sitecore.Web.Authentication;
using System.Collections.Generic;
using Sitecore.Security.Accounts;
using System.Text.RegularExpressions;
using SitecoreCognitiveServices.Feature.OleChat.Statics;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Factories;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;
using SitecoreCognitiveServices.Feature.OleChat.Intents.Parameters;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents
{
    public class KickUserIntent : BaseOleIntent
    {
        protected readonly IAuthenticationWrapper AuthenticationWrapper;

        public override string Name => "kick user";

        public override string Description => Translator.Text("Chat.Intents.KickUser.Name");

        public override bool RequiresConfirmation => true;
        
        #region Local Properties

        protected string UserKey = "Domain User";
        
        #endregion

        public KickUserIntent(
            IAuthenticationWrapper authWrapper,
            IIntentInputFactory inputFactory,
            IConversationResponseFactory responseFactory,
            IParameterResultFactory resultFactory,
            IOleSettings settings) : base(inputFactory, responseFactory, settings)
        {
            AuthenticationWrapper = authWrapper;

            ConversationParameters.Add(new UserParameter(UserKey, inputFactory, authWrapper, resultFactory));
        }

        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation) {
            
            if (!AuthenticationWrapper.IsCurrentUserAdministrator())
                return ConversationResponseFactory.Create(Name, Translator.Text("Chat.Intents.KickUser.MustBeAdminMessage"));

            var userSession = (DomainAccessGuard.Session)conversation.Data[UserKey];
            var name = userSession.UserName;
            AuthenticationWrapper.Kick(userSession.SessionID);
            
            return ConversationResponseFactory.Create(Name, string.Format(Translator.Text("Chat.Intents.KickUser.Response"), name));
        }
    }
}