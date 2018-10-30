using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;


namespace DurableFunctionDemo
{
	public static class WFActivities
	{
		public const string TRAVELREGISTRATION = "TravelRegistration";
		public const string TRAVELBOOKING = "TravelBooking";
		public const string TRAVELCONFIRMATION = "TravelConfirmation";
		public const string BLOBSTORAGECONNSTRING = "UseDevelopmentStorage=true";

		[FunctionName(TRAVELREGISTRATION)]
		public static async Task<TravelPlan> TravelRegistration(
			[ActivityTrigger] TravelPlan upComingTrip,
			[Table("TripOutings", Connection = "AzureWebJobsStorage")] IAsyncCollector<TravelPlan> trips,
 			ILogger log)
		{
			// RowKey | PartitionKey | ETag {null}
			log.LogInformation($"{TRAVELREGISTRATION}:{upComingTrip.TripName}");
			log.LogInformation($"{TRAVELREGISTRATION}:add registration fee 10%");

			// function custom binding for storage access - table

			// table storage entity mapping (from|to runtime context business object)
			// before inserting a table entity record
			upComingTrip = WFBusinessData.AdminProcess(upComingTrip, 0.10m);
			upComingTrip.PartitionKey = "YourTrips";
			upComingTrip.RowKey = Guid.NewGuid().ToString();
			// upComingTrip.ETag = "*";

			await trips.AddAsync(upComingTrip);
			await Task.Delay(5000);

			return upComingTrip;
		}

		[FunctionName(TRAVELBOOKING)]
		public static async Task<TravelPlan> TravelBooking(
			[ActivityTrigger] TravelPlan upComingTrip,
			[Queue("YourTrips", Connection = "AzureWebJobsStorage")] IAsyncCollector<TravelPlan> trips,
			ILogger log)
		{
			log.LogInformation($"{TRAVELBOOKING}:{upComingTrip.TripName}");
			log.LogInformation($"{TRAVELBOOKING}:add booking fee 5%");
			// function custom binding for storage access - queue
			upComingTrip = WFBusinessData.AdminProcess(upComingTrip, 0.05m);
			await trips.AddAsync(upComingTrip);
			await Task.Delay(5000);

			return upComingTrip;
		}

		[FunctionName(TRAVELCONFIRMATION)]
		public static async Task TravelConfirmation(
			[ActivityTrigger] TravelPlan upComingTrip,
			[Blob("tripsconfirmation", Connection = "AzureWebJobsStorage")]CloudBlobContainer container,
			ILogger log)
		{
			await container.CreateIfNotExistsAsync();

			var blobRef = container.GetBlockBlobReference($"{upComingTrip.TripName}{Guid.NewGuid().ToString()}.txt");
			
			// function custom binding for storage access - blob
			upComingTrip = WFBusinessData.AdminProcess(upComingTrip, 0.05m);

			await blobRef.UploadTextAsync($"Confirmation of travel trip: {upComingTrip.TripName} | {upComingTrip.TravelBy} | {upComingTrip.TravelDistance} miles | ${upComingTrip.TravelCost} Have a nice trip!");

			log.LogInformation($"{TRAVELCONFIRMATION}:{upComingTrip.TripName}");
			log.LogInformation($"{TRAVELCONFIRMATION}:add closing fee 5%");
		}

		/* Version 2
		[FunctionName(TRAVELCONFIRMATION)]
		public static async Task TravelConfirmation(
			[ActivityTrigger] TravelPlan upComingTrip, 
			ILogger log)
		{
			if (CloudStorageAccount.TryParse(BLOBSTORAGECONNSTRING, out var storageAccount))
			{
				var container = storageAccount.CreateCloudBlobClient().GetContainerReference("tripsconfirmation");
				await container.CreateIfNotExistsAsync();

				var blobRef = container.GetBlockBlobReference($"{upComingTrip.TripName}{Guid.NewGuid().ToString()}.txt");

				// function custom binding for storage access - blob
				upComingTrip = WFBusinessData.AdminProcess(upComingTrip, 0.05m);

				log.LogInformation($"{TRAVELCONFIRMATION}:{upComingTrip.TripName}");
				log.LogInformation($"{TRAVELCONFIRMATION}:add closing fee 5%");

				await blobRef.UploadTextAsync($"Confirmation of travel trip: {upComingTrip.TripName} | {upComingTrip.TravelBy} | {upComingTrip.TravelDistance} miles | ${upComingTrip.TravelCost} Have a nice trip!");
				await Task.Delay(5000);
			}
			else
			{
				log.LogError($"{TRAVELCONFIRMATION}:unable to produce confirmation document"); 
				log.LogError($"{TRAVELCONFIRMATION}:failed {upComingTrip.TripName}");
			}
		} */
	}
}
