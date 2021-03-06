﻿using System;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;
using Sitecore.Data.Items;
using SitecoreCognitiveServices.Foundation.SCSDK.Commands;

namespace SitecoreCognitiveServices.Feature.OleChat.Commands
{
    [Serializable]
    public class BackupOle : BaseOleChatCommand
    {
        public virtual void Run(ClientPipelineArgs args)
        {
            if (args.IsPostBack)
                return;
            
            ModalDialogOptions mdo = new ModalDialogOptions($"/SitecoreCognitiveServices/OleChat/BackupOle?id={Id}&language={Language}&db={Db}")
            {
                Header = "Backup Ole",
                Height = "200",
                Width = "410",
                Message = "Backup Ole",
                Response = true
            };
            SheerResponse.ShowModalDialog(mdo);
            args.WaitForPostBack();
        }

        public override CommandState QueryState(CommandContext context)
        {
            if (Settings.MissingKeys())
                return CommandState.Disabled;

            Item ctxItem = DataWrapper?.ExtractItem(context);
            if (ctxItem == null || ctxItem.ID.Guid != Settings.OleSettingsFolderId.Guid)
                return CommandState.Hidden;

            return CommandState.Enabled;
        }
    }
}