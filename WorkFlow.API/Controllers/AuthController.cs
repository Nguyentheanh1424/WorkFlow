using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess
                ? Ok(result)
                : BadRequest(result);
        }

        [HttpPost("VerifyRegisterOtp")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyRegisterOtpCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess
                ? Ok(result)
                : BadRequest(result);
        }

        [HttpPost("ResendRegisterOtp")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendOtp([FromBody] ResendRegisterOtpCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess
                ? Ok(result)
                : BadRequest(result);
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess
                ? Ok(result)
                : BadRequest(result);
        }

        [HttpPost("RefreshToken")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess
                ? Ok(result)
                : BadRequest(result);
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess
                ? Ok(result)
                : BadRequest(result);
        }
        [HttpPost("ResetPasswordSendOtp")]
        public async Task<IActionResult> SendResetOtp()
        {
            var result = await _mediator.Send(new ResetPasswordSendOtpCommand());
            return result.IsSuccess 
                ? Ok(result) 
                : BadRequest(result);
        }

        [HttpPost("ResetPasswordVerify")]
        public async Task<IActionResult> VerifyResetPassword([FromBody] ResetPasswordVerifyCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess 
                ? Ok(result) 
                : BadRequest(result);
        }
        [HttpPost("ForgotPasswordSendOtp")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPasswordSendOtp([FromBody] ForgotPasswordSendOtpCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess 
                ? Ok(result) 
                : BadRequest(result);
        }
        [HttpPost("ForgotPasswordVerify")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPasswordVerify([FromBody] ForgotPasswordVerifyCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess 
                ? Ok(result) 
                : BadRequest(result);
        }
    }
}
