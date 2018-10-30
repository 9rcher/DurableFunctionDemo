using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace DurableFunctionDemo
{
    public static class WFListener
    {
        [FunctionName("BookingListener")]
        public static async Task BookingRun([QueueTrigger("YourTrips", Connection = "AzureWebJobsStorage")]TravelPlan myQueueItem, 
			[Table("TripBookings", Connection = "AzureWebJobsStorage")] IAsyncCollector<TravelPlan> tripBookings,
			ILogger log)
        {
            log.LogInformation($"BookingConfirmation: {myQueueItem.TripName}");

			await tripBookings.AddAsync(myQueueItem);

			// blob storage output
			if (CloudStorageAccount.TryParse(WFActivities.BLOBSTORAGECONNSTRING, out var storageAccount))
			{
				var container = storageAccount.CreateCloudBlobClient().GetContainerReference("tripsbookings");

				await container.CreateIfNotExistsAsync();

				var blobRef = container.GetBlockBlobReference($"{myQueueItem.TripName}{Guid.NewGuid().ToString()}.txt");

				log.LogInformation($"Booking receipt:{myQueueItem.TripName}");

				await blobRef.UploadTextAsync($"Receipt of travel trip: {myQueueItem.TripName} | {myQueueItem.TravelBy} | {myQueueItem.TravelDistance} miles | ${myQueueItem.TravelCost} Have a nice trip!");
				await Task.Delay(5000);
			}
		}
    }
}
