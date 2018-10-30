using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace DurableFunctionDemo
{
    public static class WFOrchestrator
    {
		[FunctionName("Traveler")]
		public static async Task<object> ProcessWF(
			[OrchestrationTrigger] DurableOrchestrationContext ctx, 
			ILogger log)
		{
			// writing code for Workflow
			var pickTrip = ctx.GetInput<TravelPlan>();

			if (!ctx.IsReplaying)
				log.LogInformation("About to call travel registration");

			var callFuncActivity1 = await
				ctx.CallActivityAsync<TravelPlan>(WFActivities.TRAVELREGISTRATION, pickTrip); 

			if (!ctx.IsReplaying)
				log.LogInformation("About to call travel booking");

			// pipeline on callFuncActivity1
			var callFuncActivity2 = await
				ctx.CallActivityAsync<TravelPlan>(WFActivities.TRAVELBOOKING, callFuncActivity1); 

			if (!ctx.IsReplaying)
				log.LogInformation("About to call travel confirmation");

			var callFuncActivity3 = await
				ctx.CallActivityAsync<TravelPlan>(WFActivities.TRAVELCONFIRMATION, callFuncActivity2); 

			return new
			{
				TravelRegistration = callFuncActivity1,
				TravelBooking = callFuncActivity2,
				TravelConfirmation = callFuncActivity3
			};
		}
    }
}
