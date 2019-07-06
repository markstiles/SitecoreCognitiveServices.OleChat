using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using System.Web.Http;
using System.Web.Mvc;
using Newtonsoft.Json;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data;
using Sitecore.Data.Managers;
using SitecoreCognitiveServices.Feature.OleChat.Areas.SitecoreCognitiveServices.Models;
using SitecoreCognitiveServices.Feature.OleChat.Factories;
using SitecoreCognitiveServices.Feature.OleChat.Services;
using SitecoreCognitiveServices.Feature.OleChat.Statics;
using SitecoreCognitiveServices.Foundation.MSSDK.Enums;
using SitecoreCognitiveServices.Foundation.MSSDK.Language.Models.Luis.Connector;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Bing;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Factories;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Speech;
using SitecoreCognitiveServices.Foundation.SCSDK.Wrappers;

namespace SitecoreCognitiveServices.Feature.OleChat.Areas.SitecoreCognitiveServices.Controllers {

    public class OleChatController : Controller
    {
        #region Constructor

        protected readonly ILuisService LuisService;
        protected readonly ILuisConversationService LuisConversationService;
        protected readonly IWebUtilWrapper WebUtil;
        protected readonly ISitecoreDataWrapper DataWrapper;
        protected readonly IOleSettings ChatSettings;
        protected readonly ISetupInformationFactory SetupFactory;
        protected readonly ISetupService SetupService;
        protected readonly ISpeechService SpeechService;
        protected readonly ISearchService Searcher;
        protected readonly IConversationContextFactory ConversationContextFactory;
        protected readonly ISpellCheckService SpellCheckService;

        protected ItemContextParameters Parameters;

        public OleChatController(
            ILuisService luisService,
            ILuisConversationService luisConversationService, 
            IWebUtilWrapper webUtil,
            ISitecoreDataWrapper dataWrapper,
            IOleSettings chatSettings,
            ISetupInformationFactory setupFactory,
            ISetupService setupService,
            ISpeechService speechService,
            ISearchService searcher,
            IConversationContextFactory conversationContextFactory,
            ISpellCheckService spellCheckService)
        {
            LuisService = luisService;
            LuisConversationService = luisConversationService;
            WebUtil = webUtil;
            DataWrapper = dataWrapper;
            ChatSettings = chatSettings;
            SetupFactory = setupFactory;
            SetupService = setupService;
            SpeechService = speechService;
            Searcher = searcher;
            ConversationContextFactory = conversationContextFactory;
            SpellCheckService = spellCheckService;
            
            ThemeManager.GetImage("Office/32x32/man_8.png", 32, 32);
            
            var lang = WebUtil.GetQueryString("language");
            if (string.IsNullOrWhiteSpace(lang))
                lang = Sitecore.Context.Language.Name;

            var db = WebUtil.GetQueryString("db");
            if (string.IsNullOrWhiteSpace(db))
                db = "master";

            Parameters = new ItemContextParameters()
            {
                Id = WebUtil.GetQueryString("id"),
                Language = lang,
                Database = db
            };
        }

        #endregion

        #region Chat

        public ActionResult Ole()
        {
            return View("Ole", Parameters);
        }

        public ActionResult Post([FromBody] Activity activity)
        {
            var s = JsonConvert.SerializeObject(activity.ChannelData);
            var d = JsonConvert.DeserializeObject<List<string>>(s);
            ItemContextParameters parameters = (d.Any())
                ? JsonConvert.DeserializeObject<ItemContextParameters>(d[0])
                : new ItemContextParameters();

            if (activity.Type == ActivityTypes.Message)
            {
                var result = !string.IsNullOrWhiteSpace(activity.Text) ? LuisService.Query(ChatSettings.OleApplicationId, activity.Text, true) : null;

                var conversationContext = ConversationContextFactory.Create(
                    ChatSettings.OleApplicationId,
                    Translator.Text("Chat.Clear"),
                    Translator.Text("Chat.ConfirmMessage"),
                    "decision - yes",
                    "decision - no",
                    "frustrated",
                    "profile user - quit",
                    activity.Text,
                    parameters,
                    result);
                var response = LuisConversationService.HandleMessage(conversationContext);
                var newMessage = Regex.Replace(response.Message, "<.*?>", " ");

                var relativePath = $"temp\\ole-{CreateMD5Hash(newMessage)}.mp3";
                var filePath = $"{Request.PhysicalApplicationPath}{relativePath}";

                var locale = SpeechLocaleOptions.enUS;
                var voice = VoiceName.EnUsGuyNeural;
                var gender = GenderOptions.Male;
                var audioFormat = AudioOutputFormatOptions.Audio24Khz160KBitRateMonoMp3;
                
                SpeechService.TextToFile(newMessage, filePath, locale, voice, gender, audioFormat);
                
                var reply = activity.CreateReply(response.Message, "en-US");
                reply.ChannelData = new ChannelData
                {
                    Input = response.Input,
                    Selections = response.Selections,
                    Action = response.Action,
                    AudioFile = $"\\{relativePath}"
                };

                return Json(reply);
            }

            return null;
        }

        public ActionResult ItemSearch(string db, string language, string query, Dictionary<string, string> parameters)
        {
            //query search
            var results = new List<SearchResultItem>();
            try
            {
                results = Searcher.GetResults(db, language, query, parameters);
            }
            catch (Exception ex)
            {

            }
            var returnList = results.Select(GetViewModelResult);
            
            return Json(new
            {
                Failed = false,
                Items = returnList
            });
        }

        public ItemSearchResultViewModel GetViewModelResult(SearchResultItem result)
        {
            var item = result.GetItem();
            var iconPath = item.Template.InnerItem.Fields[Sitecore.FieldIDs.Icon].Value;


            var model = new ItemSearchResultViewModel
            {
                Title = result.Name,
                Path = item.Paths.FullPath,
                Icon = ThemeManager.GetImage(iconPath, 32, 32)
            };

            return model;
        }

        public ActionResult SpellCheck(string text)
        {
            var newText = text;
            var tokens = SpellCheckService.SpellCheck(text, SpellCheckModeOptions.Spell).FlaggedTokens;
            foreach(var t in tokens)
            {
                newText = newText.Replace(t.Token, t.Suggestions.OrderByDescending(a => a.Score).First().Suggestion);
            }

            return Json(new { text = newText });
        }

        public string CreateMD5Hash(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        public ActionResult GetSpeechToken()
        {
            var key = "Ole-SpeechToken";
            var isInCache = HttpRuntime.Cache[key] != null;
            var token = isInCache
                ? HttpRuntime.Cache[key]
                : SpeechService.GetSpeechToken();

            if(!isInCache)
                HttpRuntime.Cache.Add(key, token, null, DateTime.UtcNow.AddMinutes(10), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
            
            return Json(new { Token = token, Region = ChatSettings.SpeechRegion, Language = SpeechLocaleOptions.enUS.MemberAttrValue() });
        }

        #endregion

        #region Backup

        public ActionResult BackupOle()
        {
            var result = SetupService.BackupOle();            

            return View("BackupOle", model: result);
        }
        
        #endregion

        #region Restore

        public ActionResult RestoreOle()
        {
            var result = SetupService.RestoreOle(true);

            return View("RestoreOle", model: result);
        }

        #endregion

        #region Setup

        public ActionResult Setup()
        {
            if (!IsSitecoreUser())
                return LoginPage();
            
            var db = Sitecore.Configuration.Factory.GetDatabase(ChatSettings.MasterDatabase);
            using (new DatabaseSwitcher(db))
            {
                ISetupInformation info = SetupFactory.Create();

                return View("Setup", info);
            }
        }
        
        public ActionResult SetupSubmit(bool overwriteOption, string luisApi, string luisApiEndpoint)
        {
            if (!IsSitecoreUser())
                return LoginPage();

            List<string> items = new List<string>();

            SetupService.SaveKeys(luisApi, luisApiEndpoint);
            
            var restoreResult = SetupService.RestoreOle(overwriteOption);
            if(!restoreResult)
                items.Add("Restore Ole");

            var queryResult = SetupService.QueryOle();
            if(!queryResult)
                items.Add("Query Ole");

            SetupService.PublishOleContent();

            return Json(new
            {
                Failed = items.Count > 0,
                Items = string.Join(",", items)
            });
        }
        
        #endregion

        #region Shared

        public bool IsSitecoreUser()
        {
            return DataWrapper.ContextUser.IsAuthenticated
                   && DataWrapper.ContextUser.Domain.Name.ToLower().Equals("sitecore");
        }

        public ActionResult LoginPage()
        {
            return new RedirectResult("/sitecore/login");
        }

        #endregion
    }
}