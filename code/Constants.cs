using Sitecore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SitecoreCognitiveServices.Feature.OleChat
{
    public static class Constants
    {
        public static class SearchParameters
        {
            public static string FilterPath = "filterPath";
            public static string TemplateId = "templateId";
            public static string AutoStart = "autostart";
        }

        public static class Paths
        {
            public static string ContentPath = "{0DE95AE4-41AB-4D01-9EB0-67441B7C2450}";
            public static string SystemPath = "{13D6D6C6-C50B-4BBD-B331-2B04F1A58F21}";
            public static string GoalPath = "{0CB97A9F-CAFB-42A0-8BE1-89AB9AE32BD9}";
            public static string ProfilePath = "{12BD7E35-437B-449C-B931-23CFA12C03D8}";
        }

        public static class TemplateIds
        {
            public static ID FolderTemplateId = new ID("{A87A00B1-E6DB-45AB-8B54-636FEC3B5523}");
            public static ID ProfileTemplateId = new ID("{8E0C1738-3591-4C60-8151-54ABCC9807D1}");
            public static ID ProfileTemplateBranchId = new ID("{07624A03-BB2F-45D8-ABE1-15E2B1705FF3}");
            public static ID ProfileKeyTemplateId = new ID("{44AB5107-3C73-42F0-A427-BEC549F944B9}");
            public static ID GoalCategoryTemplateId = new ID("{DB6E13B8-786C-4DD6-ACF2-3E5E6A959905}");
            public static ID GoalTemplateId = new ID("{475E9026-333F-432D-A4DC-52E03B75CB6B}");
            public static ID ProfileCardTemplateId = new ID("{0FC09EA4-8D87-4B0E-A5C9-8076AE863D9C}");
            public static ID PatternCardTemplateId = new ID("{4A6A7E36-2481-438F-A9BA-0453ECC638FA}");
        }

        public static class FieldIds
        {
            public static class StandardFields
            {
                public static ID TrackingFieldId = new ID("{B0A67B2A-8B07-4E0B-8809-69F751709806}");
            }

            public static class Profile
            {
                public static ID NameFieldId = new ID("{5D9FE5D7-4C45-4A98-A1F5-4796A6DA428B}");
                public static ID TypeFieldId = new ID("{9FB30799-29EF-46AF-910D-188BD16314B1}");
                public static ID DecayFieldId = new ID("{EB6BDFDC-44EC-449C-854B-823F5AF4BE97}");
            
            }

            public static class ProfileKey
            {
                public static ID NameFieldId = new ID("{2E1A9AAB-F7C1-49EE-B032-28B98D3688D6}");
                public static ID MinValueFieldId = new ID("{271D7B0F-446C-4F24-BFE7-E33C20BF7A41}");
                public static ID MaxValueFieldId = new ID("{12E2C64A-5F08-4948-86C8-55AE06AFEEF8}");
            }

            public static class ProfileCard
            {
                public static ID NameFieldId = new ID("{A26FC256-1DB0-44AA-BA3D-1C830BF882A2}");
                public static ID ProfileCardValueFieldId = new ID("{85970AB7-22EA-4206-BE86-C0167178860B}");
            }

            public static class PatternCard
            {
                public static ID NameFieldId = new ID("{FE732790-2DDB-4657-A882-C2FCE1FBF0C9}");
                public static ID PatternFieldId = new ID("{6DA71AD3-5849-4482-9317-D21390E5A348}");
            }
        }

        public static class ItemIds
        {
            public static ID ProfileNodeId = new ID("{12BD7E35-437B-449C-B931-23CFA12C03D8}");
            public static ID GoalNodeId = new ID("{0CB97A9F-CAFB-42A0-8BE1-89AB9AE32BD9}");
        }
    }
}