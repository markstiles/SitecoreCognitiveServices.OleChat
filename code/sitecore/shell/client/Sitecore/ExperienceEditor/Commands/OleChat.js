define(["sitecore", "/-/speak/v1/ExperienceEditor/ExperienceEditor.js"], function (Sitecore, ExperienceEditor) {
    Sitecore.Commands.OleChat =
    {
        canExecute: function (context)
        {
            return (!ExperienceEditor.isInMode("edit") || context.currentContext.isFallback)
                ? false
                : true;
        },
        execute: function (context) {
            var dialogPath = "/SitecoreCognitiveServices/OleChat/Ole?id=" + decodeURI(context.currentContext.itemId) + "&language=" + context.currentContext.language + "&db=" + context.currentContext.database;
            var dialogFeatures = "dialogHeight: 500px;dialogWidth: 810px;";
            ExperienceEditor.Dialogs.showModalDialog(dialogPath, '', dialogFeatures, null, function () {});
        }
    };
});