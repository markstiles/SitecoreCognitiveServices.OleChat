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

namespace SitecoreCognitiveServices.Feature.OleChat.Intents
{
    public class ThanksIntent : BaseOleIntent
    {
        public override string Name => "thanks";

        public override string Description => "";

        public override bool RequiresConfirmation => false;

        public ThanksIntent(
            IIntentInputFactory inputFactory,
            IConversationResponseFactory responseFactory,
            IOleSettings settings) : base(inputFactory, responseFactory, settings)
        {
        }

        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation)
        {
            List<string> responses = new List<string>()
            {
                Translator.Text("Chat.Intents.Thanks.1"),
                Translator.Text("Chat.Intents.Thanks.2"),
                Translator.Text("Chat.Intents.Thanks.3"),
                Translator.Text("Chat.Intents.Thanks.4"),
                Translator.Text("Chat.Intents.Thanks.5"),
                Translator.Text("Chat.Intents.Thanks.6"),
                Translator.Text("Chat.Intents.Thanks.7")
            };

            return ConversationResponseFactory.Create(Name, responses[new Random().Next(0, responses.Count)]);
        }
    }
}