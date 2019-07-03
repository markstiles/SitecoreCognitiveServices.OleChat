using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using SitecoreCognitiveServices.Foundation.MSSDK.Language.Models.Luis;
using SitecoreCognitiveServices.Feature.OleChat.Statics;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Factories;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents.Self
{
    public class AboutIntent : BaseOleIntent
    {
        protected readonly IServiceProvider Provider;
        
        public override string KeyName => "self - about";

        public override string DisplayName => Translator.Text("Chat.Intents.About.Name");

        public override bool RequiresConfirmation => false;

        public AboutIntent(
            IOleSettings settings,
            IIntentInputFactory inputFactory,
            IConversationResponseFactory responseFactory,
            IServiceProvider provider) : base(inputFactory, responseFactory, settings) {
            Provider = provider;
        }
        
        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation)
        {
            var intents = Provider.GetServices<IIntent>()
                .Where(g => g.ApplicationId.Equals(ApplicationId) && !g.DisplayName.Equals(""))
                .OrderBy(b => b.DisplayName)
                .Select(i => $"<li>{i.DisplayName}</li>");
                
            var str = string.Join("", intents);

            return ConversationResponseFactory.Create(KeyName, $"{Translator.Text("Chat.Intents.About.Response")}: <br/><ul>{str}</ul>");
        }
    }
}