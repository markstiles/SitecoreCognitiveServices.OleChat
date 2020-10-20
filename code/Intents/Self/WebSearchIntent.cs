using SitecoreCognitiveServices.Foundation.MSSDK.Bing;
using SitecoreCognitiveServices.Foundation.MSSDK.Language.Models.Luis;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Enums;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Factories;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml.Linq;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents.Self
{
    public class WebSearchIntent : BaseOleIntent
    {
        public override string KeyName => "self - websearch";

        public override string DisplayName => "";

        public override bool RequiresConfirmation => false;

        protected IWebSearchRepository WebSearchRepository;

        protected readonly HttpContextBase Context;

        public WebSearchIntent(
            HttpContextBase context,
            IIntentInputFactory inputFactory,
            IConversationResponseFactory responseFactory,
            IWebSearchRepository webSearchRepository,
            IOleSettings settings) : base(inputFactory, responseFactory, settings)
        {
            Context = context;
            WebSearchRepository = webSearchRepository;
        }

        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation)
        {
            var message = "Hmmm. I'm not sure so I've looked up some possible answers for you";

            var path = Context.Server.MapPath("~/sitecore/shell/sitecore.version.xml");
            if (!File.Exists(path))
                return ConversationResponseFactory.Create(KeyName, string.Empty);

            string xmlText = File.ReadAllText(path);
            XDocument xdoc = XDocument.Parse(xmlText);

            var version = xdoc.Descendants("version").First();
            var major = version.Descendants("major").First().Value;
            var minor = version.Descendants("minor").First().Value;
            
            var searchItems = WebSearchRepository
                .WebSearch($"{result.Query} in Sitecore {major}.{minor}");
            
            var options = searchItems.WebPages.Value
                .Take(3)
                .Select(a => new ListItem(a.Name, a.Url))
                .ToList();

            var intentInput = IntentInputFactory.Create(IntentInputType.ExternalLinks, options);
            
            return ConversationResponseFactory.Create(KeyName, message, true, intentInput);
        }
    }
}