using WorkFlow.Application.Common.Interfaces.Services;

namespace WorkFlow.Infrastructure.Services
{
    public class DateTimeService : IDateTimeService
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
