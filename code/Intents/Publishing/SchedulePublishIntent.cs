using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Sitecore;
using SitecoreCognitiveServices.Foundation.MSSDK.Language.Models.Luis;
using SitecoreCognitiveServices.Foundation.SCSDK.Wrappers;
using Sitecore.Data;
using Sitecore.Data.Items;
using SitecoreCognitiveServices.Feature.OleChat.Statics;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Factories;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;
using SitecoreCognitiveServices.Feature.OleChat.Intents.Parameters;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents.Publishing
{
    public class SchedulePublishIntent : BaseOleIntent
    {
        protected readonly ISitecoreDataWrapper DataWrapper;
        protected readonly IPublishWrapper PublishWrapper;
        
        public override string KeyName => "publishing - schedule publish";

        public override string DisplayName => Translator.Text("Chat.Intents.SchedulePublish.Name");

        public override bool RequiresConfirmation => true;
        
        #region Local Properties

        protected string ItemKey = "Item Path";
        protected string VersionKey = "Version";
        protected string DateKey = "Date";
        protected string DBKey = "Database Names";

        #endregion

        public SchedulePublishIntent(
            IOleSettings settings,
            ISitecoreDataWrapper dataWrapper,
            IIntentInputFactory inputFactory,
            IConversationResponseFactory responseFactory,
            IParameterResultFactory resultFactory,
            IPublishWrapper publishWrapper) : base(inputFactory, responseFactory, settings)
        {
            DataWrapper = dataWrapper;
            PublishWrapper = publishWrapper;

            ConversationParameters.Add(new ItemParameter(ItemKey, dataWrapper, inputFactory, resultFactory));
            ConversationParameters.Add(new ItemVersionParameter(VersionKey, ItemKey, inputFactory, resultFactory));
            ConversationParameters.Add(new DateParameter(DateKey, Translator.Text("Chat.Intents.SchedulePublish.DateParameterRequest"), inputFactory, dataWrapper, resultFactory));
            ConversationParameters.Add(new DatabaseParameter(DBKey, settings, dataWrapper, inputFactory, publishWrapper, resultFactory));
        }
        
        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation)
        {
            var item = (Item)conversation.Data[ItemKey];
            var version = (string)conversation.Data[VersionKey];
            var date = (string)conversation.Data[DateKey];
            var dbs = (List<Database>)conversation.Data[DBKey];

            int versionInt;
            int.TryParse(version, out versionInt);
            
            var fields = (versionInt == 0)
                ? GetFields(date, "") //set item settings
                : GetFields("", date); //version settings
        
            DataWrapper.UpdateFields(item, fields);

            var db = Sitecore.Configuration.Factory.GetDatabase(parameters.Database);
            var langs = DataWrapper.GetLanguages(db)
                .Where(a => a.Name.Equals(parameters.Language))
                .ToArray();
            PublishWrapper.PublishItem(item, dbs.ToArray(), langs, false, false, false);

            var dateResponse = "";
            if (!string.IsNullOrWhiteSpace(date))
            {
                var dateTime = DateUtil.IsoDateToDateTime(date);
                dateResponse = (dateTime.Hour > 0)
                    ? dateTime.ToString("MMMM d, yyyy h:mmtt")
                    : dateTime.ToString("MMMM d, yyyy ");

                var at = Translator.Text("Chat.Intents.SchedulePublish.At");
                dateResponse = $" {at} {dateResponse}";
            }
            return ConversationResponseFactory.Create(KeyName, string.Format(Translator.Text("Chat.Intents.SchedulePublish.Response"), item.Paths.Path, string.Join(", ", dbs.Select(a => a.Name)), dateResponse));
        }

        public virtual Dictionary<ID, string> GetFields(string publishDate, string validFrom)
        {
            var fields = new Dictionary<ID, string>
            {
                { FieldIDs.PublishDate, publishDate },
                { FieldIDs.ValidFrom, validFrom },
            };

            return fields;
        }       
    }
}