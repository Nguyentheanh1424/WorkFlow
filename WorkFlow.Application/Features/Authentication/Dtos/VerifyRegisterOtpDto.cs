namespace WorkFlow.Application.Features.Authentication.Dtos
{
    public class VerifyRegisterOtpDto
    {
        public string Email { get; set; } = String.Empty;
        public string Otp { get; set; } = String.Empty;
    }
}
