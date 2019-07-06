using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.ContentSearch.Linq;
using Sitecore.ContentSearch.Linq.Utilities;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.ContentSearch.Security;
using Sitecore.ContentSearch.Utilities;
using Sitecore.Data;
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

        public virtual List<SearchResultItem> GetResults(string db, string language, string query, Dictionary<string, string> parameters)
        {
            var indexName = ContentSearch.GetSitecoreIndexName(db);
            var index = ContentSearch.GetIndex(indexName);
            using (var context = index.CreateSearchContext(SearchSecurityOptions.EnableSecurityCheck))
            {

                var queryable = context.GetQueryable<SearchResultItem>()
                    .Where(a => a.Language == language);

                if (parameters.ContainsKey(Constants.SearchParameters.FilterPath))
                {
                    var path = new ID(parameters[Constants.SearchParameters.FilterPath]);
                    queryable = queryable.Where(c => c.Paths.Contains(path));
                }

                if (parameters.ContainsKey(Constants.SearchParameters.TemplateId))
                {
                    var idFilter = new ID(parameters[Constants.SearchParameters.TemplateId]);
                    queryable = queryable.Where(c => c.TemplateId == idFilter);
                }

                var contentPredicate = PredicateBuilder.False<SearchResultItem>();
                if (query.Contains(" "))
                {
                    var words = query.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
                    words.ForEach(w => contentPredicate = contentPredicate
                        .Or(item => item.Name.Like(w).Boost(10) 
                            || item.Name.Contains(w).Boost(10)
                            || item.Content.Like(w).Boost(5)
                            || item.Content.Contains(w).Boost(5)));
                }
                else
                {
                    contentPredicate = contentPredicate
                        .Or(item => item.Name.Like(query).Boost(10)
                            || item.Name.Contains(query).Boost(10)
                            || item.Content.Like(query).Boost(5)
                            || item.Content.Contains(query).Boost(5));
                }

                return queryable
                    .Where(contentPredicate)
                    .Take(10)
                    .ToList();
            }
        }
    }
}