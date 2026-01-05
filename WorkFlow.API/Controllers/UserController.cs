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

        [HttpGet]
        [SwaggerOperation(
            Summary = "Lấy danh sách người dùng",
            Description = "Hỗ trợ tìm kiếm theo tên/số điện thoại/email và phân trang"
        )]
        public async Task<IActionResult> GetUsers([FromQuery] GetUsersRequest request)
        {
            var query = new GetUsersQuery(
                request.Search,
                request.PageIndex,
                request.PageSize
            );

            var result = await _mediator.Send(query);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
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
            Summary = "Cập nhật avatar",
            Description =
                "Upload avatar cho người dùng hiện tại.\n\n" +
                "- Request: PUT multipart/form-data\n" +
                "  - Query: isRandom (bool). Nếu true: hệ thống tự sinh avatar, không cần gửi file.\n" +
                "  - Form-data: file (image/jpeg|image/png|image/webp) khi isRandom=false.\n\n" +
                "- Response: trả về avatarUrl (public URL từ storage). FE hiển thị bằng cách gán trực tiếp vào src của <img> " +
                "hoặc fetch GET đến avatarUrl (không cần token nếu bucket public)."
            )]
        [SwaggerResponse(StatusCodes.Status200OK, "Thành công. Response có avatarUrl để FE dùng trực tiếp.")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Dữ liệu không hợp lệ.")]
        public async Task<IActionResult> UpdateAvatar(
            [FromQuery] bool isRandom,
            [FromForm] UpdateUserAvatarRequest request)
        {
            var command = new UpdateUserAvatarCommand(
                IsRandom: isRandom,
                File: request.File
            );
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

    public class GetUsersRequest
    {
        public string? Search { get; set; }

        public int PageIndex { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }

    public class UpdateUserNameRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateUserAvatarRequest
    {
        [SwaggerSchema(Description = "File ảnh avatar")]
        public IFormFile File { get; set; } = default!;
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
