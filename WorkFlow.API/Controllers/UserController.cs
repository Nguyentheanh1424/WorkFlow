using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WorkFlow.Application.Features.Users.Commands;
using WorkFlow.Application.Features.Users.Queries;

namespace WorkFlow.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("Me")]
        [SwaggerOperation(
            Summary = "Lấy thông tin người dùng hiện tại"
        )]
        public async Task<IActionResult> GetMe()
        {
            var result = await _mediator.Send(new GetCurrentUserQuery());
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("Me/Name")]
        [SwaggerOperation(
            Summary = "Cập nhật tên người dùng",
            Description = "Chỉ được đổi tên sau mỗi 1836 ngày."
        )]
        public async Task<IActionResult> UpdateName([FromBody] UpdateUserNameRequest request)
        {
            var command = new UpdateUserNameCommand(request.Name);
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("Me/Avatar")]
        [SwaggerOperation(
            Summary = "Cập nhật avatar người dùng"
        )]
        public async Task<IActionResult> UpdateAvatar([FromBody] UpdateUserAvatarRequest request)
        {
            var command = new UpdateUserAvatarCommand(request.AvatarUrl);
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("Me/Phone")]
        [SwaggerOperation(
            Summary = "Cập nhật số điện thoại"
        )]
        public async Task<IActionResult> UpdatePhone([FromBody] UpdateUserPhoneRequest request)
        {
            var command = new UpdateUserPhoneNumberCommand(request.PhoneNumber);
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("Me/DateOfBirth")]
        [SwaggerOperation(
            Summary = "Cập nhật ngày sinh"
        )]
        public async Task<IActionResult> UpdateDateOfBirth([FromBody] UpdateUserDateOfBirthRequest request)
        {
            var command = new UpdateUserDateOfBirthCommand(request.DateOfBirth);
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }

    public class UpdateUserNameRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateUserAvatarRequest
    {
        public string AvatarUrl { get; set; } = string.Empty;
    }

    public class UpdateUserPhoneRequest
    {
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class UpdateUserDateOfBirthRequest
    {
        public DateTime DateOfBirth { get; set; }
    }
}
