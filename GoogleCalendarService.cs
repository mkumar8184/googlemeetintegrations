using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;

namespace WebApplication1
{
    public class GoogleCalendarService
    {
        private readonly CalendarService _service;

        public GoogleCalendarService()
        {
            var credential = GetCredentials().Result;
            _service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "manojmeet",
            });
        }

        private async Task<UserCredential> GetCredentials()
        {
            using (var stream = new FileStream("credential.json", FileMode.Open, FileAccess.Read))
            {
                return await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { CalendarService.Scope.Calendar },
                    "user",
                    CancellationToken.None);
            }
        }

        public async Task<Event> CreateEventWithMeetLink(Event newEvent)
        {
            var calendarId = "primary"; // Use 'primary' for the authenticated user's calendar
            var request = _service.Events.Insert(newEvent, calendarId);
            request.ConferenceDataVersion = 1; // Request conference data
            return await request.ExecuteAsync();
        }
        public async Task<Events> GetUpcomingEventsAsync(string calendarId = "primary")
        {
            // Define the time period to query for events
            var request = _service.Events.List(calendarId);
            request.TimeMin = DateTime.UtcNow;
            request.TimeMax = DateTime.UtcNow.AddMonths(1); // Get events for the next month
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            return await request.ExecuteAsync();
        }

        public async Task<Event> GetEventByIdAsync(string calendarId, string eventId)
        {
            return await _service.Events.Get(calendarId, eventId).ExecuteAsync();
        }

        public async Task DeleteEventAsync(string calendarId, string eventId)
        {
            await _service.Events.Delete(calendarId, eventId).ExecuteAsync();
        }
    }
}
