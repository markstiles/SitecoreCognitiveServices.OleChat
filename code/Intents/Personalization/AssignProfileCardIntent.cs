using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using SitecoreCognitiveServices.Foundation.MSSDK.Language.Models.Luis;
using SitecoreCognitiveServices.Foundation.SCSDK.Wrappers;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using SitecoreCognitiveServices.Feature.OleChat.Statics;
using Sitecore.Data.Managers;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Enums;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Factories;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;
using SitecoreCognitiveServices.Feature.OleChat.Intents.Parameters;
using System.Text;
using System.Xml.Linq;
using SitecoreCognitiveServices.Feature.OleChat.Services;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents.Personalization
{
    public class AssignProfileCardIntent : BaseOleIntent
    {
        protected readonly ISitecoreDataWrapper DataWrapper;
        protected readonly IPublishWrapper PublishWrapper;
        protected readonly IProfileService ProfileService;

        public override string KeyName => "personalization - assign profile card";

        public override string DisplayName => Translator.Text("Chat.Intents.CreateProfile.Name");

        public override bool RequiresConfirmation => true;

        #region Local Properties

        protected string ProfileCardItemKey = "Profile Card";
        protected string PageItemKey = "Page";
        
        #endregion

        public AssignProfileCardIntent(
            IOleSettings settings,
            ISitecoreDataWrapper dataWrapper,
            IIntentInputFactory inputFactory,
            IConversationResponseFactory responseFactory,
            IParameterResultFactory resultFactory,
            IPublishWrapper publishWrapper,
            IProfileService profileService) : base(inputFactory, responseFactory, settings)
        {
            DataWrapper = dataWrapper;
            PublishWrapper = publishWrapper;
            ProfileService = profileService;

            var profileParameters = new Dictionary<string, string>
            {
                { Constants.SearchParameters.FilterPath, Constants.Paths.ProfilePath },
                { Constants.SearchParameters.TemplateId, Constants.TemplateIds.ProfileTemplateId.ToString() }
            };
            ConversationParameters.Add(new ItemParameter(ProfileCardItemKey, "What profile card do you want to assign?", profileParameters, dataWrapper, inputFactory, resultFactory));
            var contentParameters = new Dictionary<string, string>
            {
                { Constants.SearchParameters.FilterPath, Constants.Paths.ContentPath }
            };
            ConversationParameters.Add(new ItemParameter(PageItemKey, "What page do you want to assign this profile card to?", contentParameters, dataWrapper, inputFactory, resultFactory));
        }

        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation)
        {
            var profileCardItem = (Item)conversation.Data[ProfileCardItemKey].Value;
            var pageItem = (Item)conversation.Data[PageItemKey].Value;
            var profileItem = ProfileService.GetProfileItem(profileCardItem);

            //get the item's tracking field and append the new goal to it
            var trackingField = pageItem.Fields[Constants.FieldIds.StandardFields.TrackingFieldId];
            XDocument xdoc = XDocument.Parse(trackingField.Value);
            if (!string.IsNullOrWhiteSpace(trackingField?.Value))
            {
                var profile = xdoc.Root.Descendants("profile");
                XElement profileNode = profile.FirstOrDefault(a => a.Attribute("id").Value == profileItem.ID.ToString());
                if (profileNode == null)
                {
                    profileNode = new XElement("profile",
                        new XAttribute("id", profileCardItem.ID.ToString()),
                        new XAttribute("name", profileCardItem.DisplayName),
                        new XAttribute("presets", $"{profileCardItem.DisplayName}|100||"));
                    xdoc.Root.Add(profileNode);
                }

                var profileCardValueField = profileCardItem.Fields[Constants.FieldIds.ProfileCard.ProfileCardValueFieldId];
                XDocument.Parse(profileCardValueField.Value);
                if (!string.IsNullOrWhiteSpace(profileCardValueField?.Value))
                {
                    var cardProfile = xdoc.Root.Descendants("profile");
                    XElement cardProfileNode = profile.FirstOrDefault(a => a.Attribute("id").Value == profileItem.ID.ToString());
                    var keys = cardProfileNode.Descendants("key");
                    foreach(var k in keys)
                    {
                        profileNode.Add(k);
                    }
                }
            }

            var pageFields = new Dictionary<ID, string>
            {
                { Constants.FieldIds.StandardFields.TrackingFieldId, xdoc.Root.ToString() }
            };
            
            /* added 2 profiles
             <tracking>  
                 <event id="{4B518240-1A88-4A9D-B71A-1C21BE173060}" name="Download brochure" />  
                 <event id="{CC2CC47C-8CC8-469D-B8C6-C11F202FD20A}" name="Google Plus One" />  
                 <profile id="{24DFF2CF-B30A-4B75-8967-2FE3DED82271}" name="Focus" presets="profile card 2|100||">    
                     <key name="Background" value="6" />    
                     <key name="Practical" value="5" />    
                     <key name="Process" value="0" />    
                     <key name="Scope" value="6" />  
                 </profile>  
                 <profile id="{BA06B827-C6F2-4748-BD75-AA178B770E83}" name="Function" presets="profile card 3|100||">    
                     <key name="Building Trust" value="4" />    
                     <key name="Call to Action" value="0" />    
                     <key name="Create Desire" value="0" />    
                     <key name="Define Concept" value="7" />  
                 </profile>
             </tracking>
             */

            /* add 2 profiles and customized function
            <tracking>  <event id="{4B518240-1A88-4A9D-B71A-1C21BE173060}" name="Download brochure" />  
                <event id="{CC2CC47C-8CC8-469D-B8C6-C11F202FD20A}" name="Google Plus One" />  
                <profile id="{24DFF2CF-B30A-4B75-8967-2FE3DED82271}" name="Focus" presets="profile card 2|100||">    
                    <key name="Background" value="6" />    
                    <key name="Practical" value="5" />    
                    <key name="Process" value="0" />    
                    <key name="Scope" value="6" />  
                </profile>  
                <profile id="{BA06B827-C6F2-4748-BD75-AA178B770E83}" name="Function">    
                    <key name="Building Trust" value="4" />    
                    <key name="Call to Action" value="1" />    
                    <key name="Create Desire" value="0" />    
                    <key name="Define Concept" value="7" />  
                </profile>
            </tracking>
            */

            // added 1 profile
            //< tracking >
            //    < profile id = "{24DFF2CF-B30A-4B75-8967-2FE3DED82271}" name = "Focus" presets = "profile card|100||" >
            //        < key name = "Background" value = "2" />
            //        < key name = "Practical" value = "1" />
            //        < key name = "Process" value = "5" />
            //        < key name = "Scope" value = "7" />
            //    </ profile >
            //    <event id="{CC2CC47C-8CC8-469D-B8C6-C11F202FD20A}" name="Google Plus One" />
            //    <event id="{28A7C944-B8B6-45AD-A635-6F72E8F81F69}" name="Instant Demo" />
            //</tracking>
            
            DataWrapper.UpdateFields(pageItem, pageFields);
            var toDb = DataWrapper.GetDatabase("web");
            PublishWrapper.PublishItem(pageItem, new[] { toDb }, new[] { DataWrapper.ContentLanguage }, true, false, false);
            
            return ConversationResponseFactory.Create(KeyName, string.Format(
                Translator.Text("Chat.Intents.AssignContentProfile.Response"),
                profileCardItem.DisplayName, pageItem.DisplayName));
        }
    }
}