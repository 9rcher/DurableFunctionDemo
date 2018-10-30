using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace DurableFunctionDemo
{
	public enum TravelBy { Air = 1, Train, Ferry, Bus, Bike, Horseback };

	public class TravelPlan : TableEntity
	{
		private decimal amount = default(decimal);
		
		public string TripName { get; set; }
		public TravelBy TravelBy { get; set; }
		public int TravelDistance { get; set; }
		public decimal TravelCost {
			get
			{
				if (amount == default(decimal))
					return amount = CalculateTripCost(this.TravelBy, this.TravelDistance);
				else
					return amount;
			}

			set
			{
				amount = value;
			}
		}

		private decimal CalculateTripCost(TravelBy travelBy, int distance)
		{
			decimal amount = 0;

			switch (travelBy)
			{
				case TravelBy.Air:
					amount = (decimal)(distance * 4.00);
					break;
				case TravelBy.Train:
					amount = (decimal)(distance * 1.50);
					break;
				case TravelBy.Ferry:
					amount = (decimal)(distance * 2.00);
					break;
				case TravelBy.Bus:
					amount = (decimal)(distance * 3.00);
					break;
				case TravelBy.Bike:
					amount = (decimal)(distance * 1.00);
					break;
				case TravelBy.Horseback:
					amount = (decimal)(distance * 5.00);
					break;
				default:
					return amount;

			}
			return amount;
		}
	}

    public class WFBusinessData
    {
		public static TravelPlan DrawTravelPlan(string tripName)
		{
			return  new TravelPlan()
			{
				TripName = tripName,
				TravelBy = (TravelBy)new Random().Next(1, 6),
				TravelDistance = new Random().Next(500, 2000),
			};
		}

		public static TravelPlan AdminProcess(TravelPlan trip, decimal rates)
		{
			trip.TravelCost = trip.TravelCost * (1 + rates);
			return trip;
		}
    }
}
