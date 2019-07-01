using System;
using SitecoreCognitiveServices.Feature.OleChat.Areas.SitecoreCognitiveServices.Models;
using SitecoreCognitiveServices.Foundation.MSSDK.Language.Models.Luis;
using SitecoreCognitiveServices.Feature.OleChat.Factories;
using SitecoreCognitiveServices.Feature.OleChat.Statics;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Factories;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents
{
    public class DefaultIntent : BaseOleIntent
    {
        public override string KeyName => "none";

        public override string DisplayName => "";

        public override bool RequiresConfirmation => false;

        public DefaultIntent(
            IIntentInputFactory inputFactory,
            IConversationResponseFactory responseFactory,
            IOleSettings settings) : base(inputFactory, responseFactory, settings) {
        }
        
        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation)
        {
            return ConversationResponseFactory.Create(KeyName, Translator.Text("Chat.Intents.Default.Response"));
        }
    }
}