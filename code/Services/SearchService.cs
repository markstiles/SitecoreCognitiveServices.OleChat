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

        public virtual List<SearchResultItem> GetResults(
            string db, 
            string language, 
            string query, 
            Dictionary<string, string> parameters,
            int skip, 
            int take)
        {
            var indexName = ContentSearch.GetSitecoreIndexName(db);
            var index = ContentSearch.GetIndex(indexName);
            using (var context = index.CreateSearchContext(SearchSecurityOptions.EnableSecurityCheck))
            {

                var queryable = context.GetQueryable<SearchResultItem>()
                    .Where(a => a.Language == language && a["_latestversion"] == "1");

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

                if (parameters.ContainsKey(Constants.SearchParameters.TemplateName))
                {
                    queryable = queryable.Where(c => c.TemplateName == parameters[Constants.SearchParameters.TemplateName]);
                }

                if (parameters.ContainsKey(Constants.SearchParameters.FieldName) 
                    && parameters.ContainsKey(Constants.SearchParameters.FieldValue))
                {
                    var fieldName = parameters[Constants.SearchParameters.FieldName];
                    var fieldValue = parameters[Constants.SearchParameters.FieldValue];
                    queryable = queryable.Where(c => c[fieldName] == fieldValue);
                }

                if (parameters.ContainsKey(Constants.SearchParameters.ItemName))
                {
                    queryable = queryable.Where(c => c.Name == parameters[Constants.SearchParameters.ItemName]);
                }
                
                var contentPredicate = PredicateBuilder.False<SearchResultItem>();
                var notEmptyQuery = !string.IsNullOrWhiteSpace(query);
                if (notEmptyQuery && query.Contains(" "))
                {
                    var words = query.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
                    words.ForEach(w => contentPredicate = contentPredicate
                        .Or(item => item.Name.Contains(w).Boost(10)
                            || item.Content.Contains(w).Boost(5)));
                }
                else if (notEmptyQuery)
                {
                    contentPredicate = contentPredicate
                        .Or(item => item.Name.Contains(query).Boost(10)
                            || item.Content.Contains(query).Boost(5));
                }

                queryable = queryable
                    .Where(contentPredicate)
                    .Skip(skip);

                if (take > 0)
                    queryable = queryable.Take(take);
                  
                return queryable.ToList();
            }
        }
    }
}