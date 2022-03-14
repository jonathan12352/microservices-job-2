using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Taxi_Booking_API.Models
{
    public class Booking
    {
        public long Id { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public string PickupPoint { get; set; }
        public float Current_Location_Latitude { get; set; }
        public float Current_Location_Longitude { get; set; }
    }
}
