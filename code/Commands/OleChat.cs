using System;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;
using Sitecore.Data.Items;
using SitecoreCognitiveServices.Foundation.SCSDK.Commands;

namespace SitecoreCognitiveServices.Feature.OleChat.Commands
{
    [Serializable]
    public class OleChat : BaseOleChatCommand
    {
        public virtual void Run(ClientPipelineArgs args)
        {
            if (args.IsPostBack)
                return;
            
            ModalDialogOptions mdo = new ModalDialogOptions($"/SitecoreCognitiveServices/OleChat/Ole?id={Id}&language={Language}&db={Db}")
            {
                Header = "Ole Chat",
                Height = "500",
                Width = "810",
                Message = "Chat with Ole, Sitecore's internal AI",
                Response = true
            };
            SheerResponse.ShowModalDialog(mdo);
            args.WaitForPostBack();
        }

        public override CommandState QueryState(CommandContext context)
        {
            return CommandState.Enabled;
        }
    }
}