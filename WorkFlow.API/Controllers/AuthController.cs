using MediatR;
using Microsoft.AspNetCore.Mvc;
using WorkFlow.Application.Features.Authentication.Commands;

namespace WorkFlow.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess
                ? Ok(result)
                : BadRequest(result);
        }

        [HttpPost("verify-register-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyRegisterOtpCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess
                ? Ok(result)
                : BadRequest(result);
        }

        [HttpPost("resend-register-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess
                ? Ok(result)
                : BadRequest(result);
        }
    }
}
