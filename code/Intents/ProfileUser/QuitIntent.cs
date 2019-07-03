using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SitecoreCognitiveServices.Feature.OleChat.Areas.SitecoreCognitiveServices.Models;
using SitecoreCognitiveServices.Foundation.MSSDK.Language.Models.Luis;
using SitecoreCognitiveServices.Feature.OleChat.Factories;
using SitecoreCognitiveServices.Feature.OleChat.Statics;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Factories;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents.ProfileUser
{
    public class QuitIntent : BaseOleIntent
    {
        public override string KeyName => "profile user - quit";

        public override string DisplayName => "";

        public override bool RequiresConfirmation => false;

        public QuitIntent(
            IOleSettings settings,
            IIntentInputFactory inputFactory,
            IConversationResponseFactory responseFactory) : base(inputFactory, responseFactory, settings)
        {
        }

        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation)
        {
            return ConversationResponseFactory.Create(KeyName, Translator.Text("Chat.Intents.Quit.Response"));
        }
    }
}