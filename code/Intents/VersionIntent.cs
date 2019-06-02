using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using SitecoreCognitiveServices.Feature.OleChat.Areas.SitecoreCognitiveServices.Models;
using SitecoreCognitiveServices.Foundation.MSSDK.Language.Models.Luis;
using SitecoreCognitiveServices.Feature.OleChat.Factories;
using SitecoreCognitiveServices.Feature.OleChat.Statics;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Factories;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents
{
    public class VersionIntent : BaseOleIntent
    {
        protected readonly HttpContextBase Context;

        public override string Name => "version";

        public override string Description => Translator.Text("Chat.Intents.Version.Name");

        public override bool RequiresConfirmation => false;

        public VersionIntent(
            HttpContextBase context,
            IIntentInputFactory inputFactory,
            IConversationResponseFactory responseFactory,
            IOleSettings settings) : base(inputFactory, responseFactory, settings) {
            Context = context;
        }

        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation) {

            var path = Context.Server.MapPath("~/sitecore/shell/sitecore.version.xml");
            if (!File.Exists(path))
                return ConversationResponseFactory.Create(Name, string.Empty);

            string xmlText = File.ReadAllText(path);
            XDocument xdoc = XDocument.Parse(xmlText);

            var version = xdoc.Descendants("version").First();
            var major = version.Descendants("major").First().Value;
            var minor = version.Descendants("minor").First().Value;
            var revision = version.Descendants("revision").First().Value;
            
            return ConversationResponseFactory.Create(Name, string.Format(Translator.Text("Chat.Intents.Version.Response"), major, minor, revision));
        }
    }
}