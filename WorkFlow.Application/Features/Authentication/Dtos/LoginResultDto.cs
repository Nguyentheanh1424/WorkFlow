namespace WorkFlow.Application.Features.Authentication.Dtos
{
    public class LoginResultDto
    {
        public Guid UserId { get; set; } = default!;
        public string Provider { get; set; } = default!;
        public string AccessToken { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
    }
}
