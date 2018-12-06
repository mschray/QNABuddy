using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.Search;
using System.Collections.Generic;
using System;

namespace NMBotHelper
{
    public static class Function2
    {
        static string searchServiceName;
        static string queryApiKey;
        static string queryIndexName;

        /// <summary>
        /// Using this enum to create a short name reference to the ugly long names coming back from the search service
        /// </summary>
        enum MyEnum
        {
            content = 0,
            meta_storage_path,
            keyphrases,
            merged_context,
            imageTags,
            celebrity,
        }

        public class SearchResult
        {
            public string content { get; set; }
            public string meta_storage_path { get; set; }
            public string[] keyphrases { get; set; }
            public string merged_content { get; set; }
            public string[] imageTags { get; set; }
            public string[] celebrity { get; set; }
        }


        public static List<SearchResult> Search(string searchString)
        {
            List<SearchResult> listItems = new List<SearchResult>();

            if (String.IsNullOrEmpty(searchString))
            {
                throw new Exception("Null or empty search string provided");
            }

            // get SearchIndexClient
            // docs for using IndexClient https://docs.microsoft.com/en-us/azure/search/search-howto-dotnet-sdk
            SearchIndexClient indexClient = new SearchIndexClient(searchServiceName, queryIndexName,
                new SearchCredentials(queryApiKey));
            var results = indexClient.Documents.Search(searchString);

            // take the generated names from Azure storage in the order they are returned and create array.  This allows me to use
            // enums for friendly names
            string[] documentDataKeys = { "content", "metadata_storage_path", "keyphrases","merged_content","imageTags","celebrities"};

            foreach (var result in results.Results)
            {
                SearchResult searchResult = new SearchResult()
                {
                    content = (string)result.Document[documentDataKeys[(int)MyEnum.content]],
                    merged_content = (string)result.Document[documentDataKeys[(int)MyEnum.merged_context]],
                    keyphrases = (string[])result.Document[documentDataKeys[(int)MyEnum.keyphrases]],
                    imageTags = (string[])result.Document[documentDataKeys[(int)MyEnum.imageTags]],
                    celebrity = (string[])result.Document[documentDataKeys[(int)MyEnum.celebrity]],
                    meta_storage_path = (string)result.Document[documentDataKeys[(int)MyEnum.meta_storage_path]],
                };

                listItems.Add(searchResult);
            }

            return listItems;

        }


        [FunctionName("Function2")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {

            log.Info("C# HTTP trigger function processed a request.");

            searchServiceName = System.Environment.GetEnvironmentVariable("AZURESEARCHSERVICE");
            queryApiKey = System.Environment.GetEnvironmentVariable("AZURESESARCHQUERYAPIKEY");
            queryIndexName = System.Environment.GetEnvironmentVariable("AZURESEARCHINDEXNAME");


            // parse query parameter
            string question = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "question", true) == 0)
                .Value;

            if (question == null)
            {
                // Get request body
                dynamic data = await req.Content.ReadAsAsync<object>();
                question = data?.name;
            }

            if (question == null)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body");
            }
            else
            {
                List<SearchResult> searchResult = Search(question);

                return req.CreateResponse(HttpStatusCode.OK,  searchResult);

            }

        }
    }
}
