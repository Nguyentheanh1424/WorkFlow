namespace WorkFlow.Application.Features.Authentication.Dtos
{
    public class VerifyRegisterOtpCommandDto
    {
        public string Email { get; private set; }
        public string Otp { get; private set; }
    }
}
