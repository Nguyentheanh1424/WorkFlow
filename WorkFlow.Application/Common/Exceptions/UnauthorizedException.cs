namespace WorkFlow.Application.Common.Exceptions
{
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message = "Truy cập trái phép.")
            : base(message)
        {
        }
    }
}
