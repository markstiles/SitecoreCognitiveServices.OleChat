using SitecoreCognitiveServices.Feature.OleChat.Statics;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Enums;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Factories;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents.Parameters
{
    public class YesOrNoParameter : IConversationParameter
    {
        #region Constructor

        public string ParamName { get; set; }
        public string ParamMessage { get; set; }

        public IIntentInputFactory IntentInputFactory { get; set; }
        public IParameterResultFactory ResultFactory { get; set; }
        
        public YesOrNoParameter(
            string paramName,
            string paramMessage,
            IIntentInputFactory inputFactory,
            IParameterResultFactory resultFactory)
        {
            ParamName = paramName;
            ParamMessage = paramMessage;
            IntentInputFactory = inputFactory;
            ResultFactory = resultFactory;
        }

        #endregion

        public IParameterResult GetParameter(string paramValue, IConversationContext context, ItemContextParameters parameters, IConversation conversation)
        {
            var isYes = context.Result.TopScoringIntent.Intent.Equals(context.YesIntentName, StringComparison.InvariantCultureIgnoreCase);
            var isNo = context.Result.TopScoringIntent.Intent.Equals(context.NoIntentName, StringComparison.InvariantCultureIgnoreCase);

            if (isYes)
                return ResultFactory.GetSuccess(true);

            if (isNo)
                return ResultFactory.GetSuccess(false);

            return ResultFactory.GetFailure(Translator.Text("Chat.Parameters.YesOrNoParameterValidationError"));
        }

        public IntentInput GetInput(ItemContextParameters parameters, IConversation conversation)
        {
            var yes = Translator.Text("Chat.Parameters.Yes");
            var no = Translator.Text("Chat.Parameters.No");
            var options = new List<ListItem> { new ListItem(yes, yes), new ListItem(no, no) };

            return IntentInputFactory.Create(IntentInputType.LinkList, options);
        }
    }
}