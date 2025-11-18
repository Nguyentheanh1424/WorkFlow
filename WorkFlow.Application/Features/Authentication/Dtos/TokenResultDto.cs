namespace WorkFlow.Application.Features.Authentication.Dtos
{
    public class TokenResultDto
    {
        public string AccessToken { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
    }
}
