using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WorkFlow.Application.Features.Authentication.Commands;

namespace WorkFlow.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("Register")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Đăng ký tài khoản",
            Description = "Tạo tài khoản mới và gửi OTP xác thực email."
        )]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("VerifyRegisterOtp")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Xác thực OTP đăng ký",
            Description = "Xác thực mã OTP được gửi đến email khi đăng ký tài khoản."
        )]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyRegisterOtpCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("ResendRegisterOtp")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Gửi lại OTP đăng ký",
            Description = "Gửi lại mã OTP cho email đăng ký trong trường hợp OTP hết hạn hoặc không nhận được."
        )]
        public async Task<IActionResult> ResendOtp([FromBody] ResendRegisterOtpCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Đăng nhập",
            Description = "Đăng nhập bằng email và mật khẩu hoặc tài khoản liên kết, trả về Access Token và Refresh Token."
        )]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("RefreshToken")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Làm mới Access Token",
            Description = "Nhận Access Token mới bằng cách gửi Refresh Token hợp lệ."
        )]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("Logout")]
        [SwaggerOperation(
            Summary = "Đăng xuất",
            Description = "Thu hồi Refresh Token hiện tại, yêu cầu phải đăng nhập."
        )]
        public async Task<IActionResult> Logout([FromBody] LogoutCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("ResetPasswordSendOtp")]
        [SwaggerOperation(
            Summary = "Gửi OTP reset password",
            Description = "Gửi OTP đến email của tài khoản đang đăng nhập để đặt lại mật khẩu."
        )]
        public async Task<IActionResult> SendResetOtp()
        {
            var result = await _mediator.Send(new ResetPasswordSendOtpCommand());
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("ResetPasswordVerify")]
        [SwaggerOperation(
            Summary = "Xác thực OTP reset password",
            Description = "Xác thực mã OTP và cập nhật mật khẩu mới."
        )]
        public async Task<IActionResult> VerifyResetPassword([FromBody] ResetPasswordVerifyCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("ForgotPasswordSendOtp")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Gửi OTP quên mật khẩu",
            Description = "Gửi mã OTP đến email người dùng để phục hồi mật khẩu."
        )]
        public async Task<IActionResult> ForgotPasswordSendOtp([FromBody] ForgotPasswordSendOtpCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("ForgotPasswordVerify")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Xác thực OTP quên mật khẩu",
            Description = "Dùng OTP để đặt lại mật khẩu mới cho tài khoản."
        )]
        public async Task<IActionResult> ForgotPasswordVerify([FromBody] ForgotPasswordVerifyCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
