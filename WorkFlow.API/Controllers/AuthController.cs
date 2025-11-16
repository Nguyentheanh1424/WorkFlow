using MediatR;
using Microsoft.AspNetCore.Mvc;
using WorkFlow.Application.Features.Workflows.Dtos;
using WorkFlow.Application.Features.Workflows.Commands;
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Domain.Entities;

namespace WorkFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IUnitOfWork _unit;

        public AuthController(IMediator mediator, IUnitOfWork unit)
        {
            _mediator = mediator;
            _unit = unit;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserDto dto)
        {
            var id = await _mediator.Send(new RegisterUserCommand(dto));
            return Ok(new { UserId = id });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginUserDto dto)
        {
            var token = await _mediator.Send(new LoginUserCommand(dto));
            return Ok(new { AccessToken = token });
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp(OtpDto dto)  
        {
            var result = await _mediator.Send(new VerifyOtpCommand(dto));
            if (!result)
                return BadRequest(new { Message = "OTP không hợp lệ." });

            return Ok(new { Message = "Xác thực OTP thành công." });
        }

        [HttpDelete("delete-user/{email}")]
        public async Task<IActionResult> DeleteUser(string email)
        {
            var repoUser = _unit.GetRepository<User, Guid>();
            var repoAuth = _unit.GetRepository<AccountAuth<Guid>, Guid>();

            var users = await repoUser.FindAsync(u => u.Email == email);
            if (!users.Any()) return NotFound();

            foreach (var u in users)
            {
                var auths = await repoAuth.FindAsync(a => a.UserId == u.Id);
                foreach (var a in auths)
                    await repoAuth.DeleteAsync(a);

                await repoUser.DeleteAsync(u);
            }

            await _unit.SaveChangesAsync();
            return Ok("User deleted");
        }
    }
}
