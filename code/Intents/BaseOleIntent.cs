using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SitecoreCognitiveServices.Foundation.MSSDK.Language.Models.Luis;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Factories;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents
{
    public abstract class BaseOleIntent : IIntent
    {
        public abstract string KeyName { get; }
        public abstract string DisplayName { get; }
        public abstract bool RequiresConfirmation { get; }
        
        public abstract ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation);

        #region Base Intent

        protected readonly IOleSettings Settings;
        protected readonly IConversationResponseFactory ConversationResponseFactory;
        protected readonly IIntentInputFactory IntentInputFactory;

        public virtual Guid ApplicationId => Settings.OleApplicationId;

        public virtual List<IRequiredConversationParameter> ConversationParameters { get; }
        
        protected BaseOleIntent(
            IIntentInputFactory inputFactory,
            IConversationResponseFactory responseFactory,
            IOleSettings settings)
        {
            Settings = settings;
            ConversationResponseFactory = responseFactory;
            IntentInputFactory = inputFactory;
            ConversationParameters = new List<IRequiredConversationParameter>();
        }
        
        #endregion
    }
}