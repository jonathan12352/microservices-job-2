using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Text.Json;

using Taxi_Booking_API.Models;
using System.IO;
using System.Text;

namespace Taxi_Booking_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController: ControllerBase
    {
        private BookingContext _context;

        private string geoApiBaseUrl = "https://freegeoip.app/json/";
        private string geoApiApiKey = "7322ce70-2741-11ec-a395-7f2b5c241db6";

        public BookingController(BookingContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBookings()
        {
            var result = await _context
                .Bookings.Select(x => x)
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Booking>> GetBookings(long id)
        {
            var bookings = await _context.Bookings.FindAsync(id);

            if (bookings == null) return NotFound();

            return Ok(bookings);
        }

        [HttpPost]
        public async Task<ActionResult<Booking>> PostBooking(Booking booking)
        {
            booking.Time = getTimeFromDateTime(booking.Date);

            var geoObj = InvokeGeoAPI();

            if (geoObj == null) return StatusCode(500);

            booking.Current_Location_Latitude = geoObj.latitude;
            booking.Current_Location_Longitude = geoObj.longitude;

            _context.Bookings.Add(booking);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                if (BookingExists(booking.Id)) return Conflict();

                throw;
            }

            return CreatedAtAction(nameof(GetBookings), new { id = booking.Id }, booking);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBooking(long id, Booking booking)
        {
            if (id != booking.Id) return BadRequest();

            var _booking = await _context.Bookings.FindAsync(id);

            if (_booking == null) return NotFound();

            var geoObj = InvokeGeoAPI();

            if (geoObj == null) return StatusCode(500);

            _booking.Date = booking.Date;
            _booking.Time = getTimeFromDateTime(booking.Date);

            _booking.PickupPoint = booking.PickupPoint;
            _booking.Current_Location_Latitude = geoObj.latitude;
            _booking.Current_Location_Longitude = geoObj.longitude;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) when (!BookingExists(id))
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(long id)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking == null) return NotFound();

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            return NoContent();

        }

        private bool BookingExists(long id) => _context.Bookings.Any(e => e.Id == id);
        private string getTimeFromDateTime(DateTime dateTime) => dateTime.ToString("HH:mm tt");

        private GeoObject InvokeGeoAPI()
        {
            var response = WebRequest
                .Create($"{geoApiBaseUrl}?apikey={geoApiApiKey}")
                .GetResponse() as HttpWebResponse;

            GeoObject result;

            using (var streamreader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                var byteArray = Encoding.UTF8.GetBytes(streamreader.ReadToEnd());
                result = JsonSerializer.Deserialize<GeoObject>(byteArray)!;
            }

            return result;
        }

    }
}
