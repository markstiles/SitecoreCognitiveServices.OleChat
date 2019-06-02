using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SitecoreCognitiveServices.Feature.OleChat.Areas.SitecoreCognitiveServices.Models;
using SitecoreCognitiveServices.Foundation.SCSDK.Wrappers;
using SitecoreCognitiveServices.Foundation.MSSDK.Language.Models.Luis;
using SitecoreCognitiveServices.Feature.OleChat.Factories;
using SitecoreCognitiveServices.Feature.OleChat.Statics;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Factories;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents
{
    public class LogoutIntent : BaseOleIntent
    {
        protected readonly HttpContextBase Context;
        protected readonly IAuthenticationWrapper AuthenticationWrapper;

        public override string Name => "logout";

        public override string Description => Translator.Text("Chat.Intents.Logout.Name");

        public override bool RequiresConfirmation => false;

        public LogoutIntent(
            HttpContextBase context,
            IAuthenticationWrapper authWrapper,
            IIntentInputFactory inputFactory,
            IConversationResponseFactory responseFactory,
            IOleSettings settings) : base(inputFactory, responseFactory, settings) {
            Context = context;
            AuthenticationWrapper = authWrapper;
        }

        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation)
        {
            return ConversationResponseFactory.Create(Name, Translator.Text("Chat.Intents.Logout.Response"), true, "logout");
        }
    }
}