using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.ContentSearch.Linq;
using Sitecore.ContentSearch.Linq.Utilities;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.ContentSearch.Security;
using Sitecore.ContentSearch.Utilities;
using SitecoreCognitiveServices.Foundation.SCSDK.Wrappers;

namespace SitecoreCognitiveServices.Feature.OleChat.Services
{
    public class SearchService : ISearchService
    {
        #region Constructor 
        
        protected readonly IContentSearchWrapper ContentSearch;

        public SearchService(
            IContentSearchWrapper contentSearch)
        {
            ContentSearch = contentSearch;
        }

        #endregion

        public virtual List<SearchResultItem> GetResults(string db, string query)
        {
            var indexName = ContentSearch.GetSitecoreIndexName(db);
            var index = ContentSearch.GetIndex(indexName);
            using (var context = index.CreateSearchContext(SearchSecurityOptions.EnableSecurityCheck))
            {
                var contentPredicate = PredicateBuilder.False<SearchResultItem>();

                if (query.Contains(" "))
                {
                    var words = query.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
                    words.ForEach(w => contentPredicate = contentPredicate
                        .Or(item => item.Content.Contains(w) || item.Name.Contains(w) || item.TemplateName.Contains(w) || item.Path.Contains(w)));
                }
                else
                {
                    contentPredicate = contentPredicate.Or(item => item.Content.Contains(query) || item.Name.Contains(query) || item.TemplateName.Contains(query) || item.Path.Contains(query));
                }

                return context.GetQueryable<SearchResultItem>()
                    .Where(contentPredicate)
                    .Take(10)
                    .ToList();
            }
        }
    }
}