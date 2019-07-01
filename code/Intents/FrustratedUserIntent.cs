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

namespace SitecoreCognitiveServices.Feature.OleChat.Intents
{
    public class FrustratedUserIntent : BaseOleIntent
    {
        public override string KeyName => "frustrated user";

        public override string DisplayName => "";

        public override bool RequiresConfirmation => false;

        public FrustratedUserIntent(
            IIntentInputFactory inputFactory,
            IConversationResponseFactory responseFactory,
            IOleSettings settings) : base(inputFactory, responseFactory, settings)
        {
        }

        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation)
        {
            List<string> responses = new List<string>()
            {
                Translator.Text("Chat.Intents.FrustratedUser.1"),
                Translator.Text("Chat.Intents.FrustratedUser.2"),
                Translator.Text("Chat.Intents.FrustratedUser.3"),
                Translator.Text("Chat.Intents.FrustratedUser.4"),
                Translator.Text("Chat.Intents.FrustratedUser.5"),
                Translator.Text("Chat.Intents.FrustratedUser.6")
            };

            return ConversationResponseFactory.Create(KeyName, responses[new Random().Next(0, responses.Count)]);
        }
    }
}