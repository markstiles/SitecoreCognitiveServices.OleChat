using System;
using System.Collections.Generic;
using System.Linq;
using SitecoreCognitiveServices.Foundation.SCSDK.Wrappers;
using SitecoreCognitiveServices.Foundation.MSSDK.Language.Models.Luis;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.ContentSearch.Security;
using SitecoreCognitiveServices.Feature.OleChat.Statics;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Factories;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents
{
    public class LockedItemCountIntent : BaseOleIntent
    {
        protected readonly IAuthenticationWrapper AuthenticationWrapper;
        protected readonly IContentSearchWrapper ContentSearchWrapper;

        public override string KeyName => "profile - locked item count";

        public override string DisplayName => Translator.Text("Chat.Intents.LockedItemCount.Name");

        public override bool RequiresConfirmation => false;

        public LockedItemCountIntent(
            IAuthenticationWrapper authWrapper,
            IContentSearchWrapper searchWrapper,
            IIntentInputFactory inputFactory,
            IConversationResponseFactory responseFactory,
            IOleSettings settings) : base(inputFactory, responseFactory, settings) {
            AuthenticationWrapper = authWrapper;
            ContentSearchWrapper = searchWrapper;
        }
        
        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation)
        {
            var items = GetCurrentUserUnlockedItems(parameters.Database);
            
            return ConversationResponseFactory.Create(KeyName, string.Format(Translator.Text("Chat.Intents.LockedItemCount.Response"), items.Count));
        }
        
        protected List<SearchResultItem> GetCurrentUserUnlockedItems(string db)
        {
            var userMod = AuthenticationWrapper.GetCurrentUser().DisplayName.Replace("\\", "").ToLower();

            using (var context = ContentSearchWrapper.GetIndex(ContentSearchWrapper.GetSitecoreIndexName(db)).CreateSearchContext(SearchSecurityOptions.DisableSecurityCheck)) {
                return context
                    .GetQueryable<SearchResultItem>()
                    .Where(a => a.LockOwner.Equals(userMod)).ToList();
            }
        }
    }
}