using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using RestSharp;


namespace NMBotHelper
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // parse query parameter
            string question = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "question", true) == 0)
                .Value;
            
            // If question isn't in the query parameter check the body
            if (question == null)
            {
                // Get request body
                dynamic data = await req.Content.ReadAsAsync<object>();
                question = data?.question;

                // if question isn't in the query parameter or body return an 
                if (question == null)
                {
                    req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a question on the query string or in the request body");
                }
            }

            string endPoint = System.Environment.GetEnvironmentVariable("QNAMAKERENDPOINT");
            string qNAMakerKey = System.Environment.GetEnvironmentVariable("QNAMAKERKEY");

            var client = new RestClient(endPoint);
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", qNAMakerKey);

            var json = new
            {
                question
            };
            request.AddParameter("undefined", JsonConvert.SerializeObject(json), ParameterType.RequestBody);

            IRestResponse response = client.Execute(request);

            var restResponse = JsonConvert.DeserializeObject<RestResponse>(response.Content);
            
            var answers = restResponse?.Answers.FirstOrDefault();
            if(answers == null)
            {
                req.CreateResponse(HttpStatusCode.BadRequest, "Answer is null");
            }
            var answer = answers.answer;
            var returnResult = new
            {
                body = $"The answer is: \'{answer}\'"
            };
            return req.CreateResponse(HttpStatusCode.OK, returnResult);
        }
    }
}
