

using Microsoft.AspNetCore.Mvc;
using Google.Apis.Calendar.v3.Data;
using System.Threading.Tasks;
using Google.Apis.Calendar.v3;
namespace WebApplication1.Controllers
{


    public class CalendarController : Controller
    {
       private readonly GoogleCalendarService _calendarService;

    public CalendarController(GoogleCalendarService calendarService)
    {
        _calendarService = calendarService;
    }

    public async Task<IActionResult> CreateEvent()
    {
            try
            {
                var events = await _calendarService.GetUpcomingEventsAsync();
                return View(events);
            }
            catch (Exception ex)
            {
                // Log the exception and return an error view or message
                ViewBag.ErrorMessage = $"An error occurred: {ex.Message}";
                return View("Error");
            }
            
    }

    [HttpPost]
    public async Task<IActionResult> CreateEvent(string summary, string description, DateTime startDate, DateTime endDate,string attendees)
        {

            // Check if startDate and endDate are valid and not empty
            if (startDate == default || endDate == default || startDate >= endDate)
            {
                ModelState.AddModelError("", "Invalid start or end date/time.");
                return View(new Events());
            }
            var attendeeEmails = attendees
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(email => email.Trim())
                .ToList();
            var newEvent = new Event()
        {
            Summary = summary,
            Description = description,
            Start = new EventDateTime()
            {
                DateTime = DateTimeOffset.Parse(startDate.ToString("yyyy-MM-ddTHH:mm:ss")).DateTime,
                TimeZone = "Asia/Kolkata",
            },
            End = new EventDateTime()
            {
                DateTime = DateTimeOffset.Parse(endDate.ToString("yyyy-MM-ddTHH:mm:ss")).DateTime,
                TimeZone = "Asia/Kolkata",
            },
                Attendees = attendeeEmails.Select(email => new EventAttendee { Email = email }).ToList(),
                ConferenceData = new ConferenceData()
                {
                    CreateRequest = new CreateConferenceRequest()
                    {
                        RequestId = Guid.NewGuid().ToString(),
                        ConferenceSolutionKey = new ConferenceSolutionKey()
                        {
                            Type = "hangoutsMeet",
                        }
                    }
                }
            };

        var createdEvent = await _calendarService.CreateEventWithMeetLink(newEvent);

        ViewBag.MeetLink = createdEvent.ConferenceData.EntryPoints[0].Uri;
        return View("EventCreated");
    }
    
        
        public async Task<IActionResult> CancelEvent(string eventId)
        {
            if (string.IsNullOrEmpty(eventId))
            {
                return BadRequest("Event ID is required.");
            }

            try
            {
                // Assume "primary" is the calendar ID, adjust if needed
                var calendarId = "primary";
                await _calendarService.DeleteEventAsync(calendarId, eventId);
                ViewBag.Message = "Event successfully canceled.";
            }
            catch (Exception ex)
            {
                // Log the exception and handle the error
                ViewBag.ErrorMessage = $"An error occurred: {ex.Message}";
            }

            return View("EventCanceled"); // Create this view to inform users of the result
        }

    }

   
}
