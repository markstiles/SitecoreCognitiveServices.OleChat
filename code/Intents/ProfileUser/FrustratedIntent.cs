using SitecoreCognitiveServices.Feature.OleChat.Factories;
using SitecoreCognitiveServices.Foundation.MSSDK.Language.Models.Luis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SitecoreCognitiveServices.Feature.OleChat.Areas.SitecoreCognitiveServices.Models;
using SitecoreCognitiveServices.Feature.OleChat.Statics;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Factories;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents.ProfileUser
{
    public class FrustratedIntent : BaseOleIntent
    {
        public override string KeyName => "frustrated";

        public override string DisplayName => "";

        public override bool RequiresConfirmation => false;

        public FrustratedIntent(
            IIntentInputFactory inputFactory,
            IConversationResponseFactory responseFactory,
            IOleSettings settings) : base(inputFactory, responseFactory, settings)
        {
        }

        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation)
        {
            List<string> responses = new List<string>()
            {
                Translator.Text("Chat.Intents.Frustrated.1"),
                Translator.Text("Chat.Intents.Frustrated.2"),
                Translator.Text("Chat.Intents.Frustrated.3"),
                Translator.Text("Chat.Intents.Frustrated.4"),
                Translator.Text("Chat.Intents.Frustrated.5"),
                Translator.Text("Chat.Intents.Frustrated.6")
            };

            return ConversationResponseFactory.Create(KeyName, responses[new Random().Next(0, responses.Count)]);
        }
    }
}