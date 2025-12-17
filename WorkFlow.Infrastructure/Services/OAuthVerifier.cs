using Google.Apis.Auth;
using System.Net.Http.Json;
using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Services;

namespace WorkFlow.Infrastructure.Services
{
    public class OAuthVerifier : IOAuthVerifier
    {
        private readonly HttpClient _http;

        public OAuthVerifier(HttpClient http)
        {
            _http = http;
        }

        public async Task<OAuthProfileDto> VerifyGoogleAsync(string token)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(token);

                if (!payload.EmailVerified)
                    throw new UnauthorizedException("Google email chưa được xác minh.");

                return new OAuthProfileDto
                {
                    Uid = payload.Subject,
                    Email = payload.Email,
                    Name = payload.Name
                };
            }
            catch
            {
                throw new UnauthorizedException("Google token không hợp lệ.");
            }
        }

        public async Task<OAuthProfileDto> VerifyFacebookAsync(string token)
        {
            var url =
                $"https://graph.facebook.com/me?fields=id,name,email&access_token={token}";

            var response = await _http.GetFromJsonAsync<FacebookMeResponse>(url);

            if (response == null || string.IsNullOrEmpty(response.Id))
                throw new UnauthorizedException("Facebook token không hợp lệ");

            return new OAuthProfileDto
            {
                Uid = response.Id,
                Email = response.Email ?? $"facebook_{response.Id}@no-email.local",
                Name = response.Name
            };
        }

        private class FacebookMeResponse
        {
            public string Id { get; set; } = default!;
            public string? Email { get; set; }
            public string? Name { get; set; }
        }
    }
}
