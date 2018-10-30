using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Host;

namespace DurableFunctionDemo
{
    public static class WFstarter
    {
        [FunctionName("WFstarter")]
        public static async Task<HttpResponseMessage> Run(
			[HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
			HttpRequestMessage req, 
			[OrchestrationClient] DurableOrchestrationClient starter, // manually added runtime components
			ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // parse query parameter
            string name = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "TripName", true) == 0)
                .Value;

            if (name == null)
            {
                // Get request body
                dynamic data = await req.Content.ReadAsAsync<object>();
                name = data?.name;
            }

			if (name == null)
			{
				log.LogInformation($"missing the trip name key on the query string or in the request body");
				return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a trip name on the query string or in the request body");
			}
			else
			{
				name = $"(Trip to: {name})";
			}

			log.LogInformation($"About to start orchestration for {name}");

			var pickTrip = WFBusinessData.DrawTravelPlan(name);

			var orchestrationId = await starter.StartNewAsync("Traveler", pickTrip); // creating an instance of orchestration

			return starter.CreateCheckStatusResponse(req, orchestrationId); // http status 202
        }
    }
}
