using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WorkFlow.Application.Features.WorkSpaces.Commands;
using WorkFlow.Application.Features.WorkSpaces.Queries;
using WorkFlow.Domain.Enums;

namespace WorkFlow.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class WorkSpaceController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WorkSpaceController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("Create")]
        [SwaggerOperation(
            Summary = "Tạo Workspace mới",
            Description = "Tạo một Workspace mới cho người dùng hiện tại."
        )]
        public async Task<IActionResult> Create([FromBody] CreateWorkspaceCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Lấy danh sách Workspace",
            Description = "Trả về danh sách Workspace mà người dùng đang tham gia. Có thể lọc theo từ khóa."
        )]
        public async Task<IActionResult> GetAll([FromQuery] string? search)
        {
            var result = await _mediator.Send(new GetWorkspacesQuery(search));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{workSpaceId}")]
        [SwaggerOperation(
            Summary = "Lấy thông tin chi tiết Workspace",
            Description = "Bao gồm thông tin cơ bản, mô tả, background và các cấu hình khác."
        )]
        public async Task<IActionResult> GetById(Guid workSpaceId)
        {
            var result = await _mediator.Send(new GetWorkspaceDetailQuery(workSpaceId));
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpPut("{workSpaceId}/Name")]
        [SwaggerOperation(
            Summary = "Cập nhật tên Workspace",
            Description = "Chỉ Workspace Owner hoặc Admin mới có quyền đổi tên Workspace."
        )]
        public async Task<IActionResult> UpdateName(Guid workSpaceId, [FromBody] UpdateNameRequest request)
        {
            var command = new UpdateWorkspaceNameCommand(workSpaceId, request.Name);
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{workSpaceId}/Description")]
        [SwaggerOperation(
            Summary = "Cập nhật mô tả Workspace",
            Description = "Chỉ Workspace Owner hoặc Admin mới có quyền cập nhật mô tả."
        )]
        public async Task<IActionResult> UpdateDescription(Guid workSpaceId, [FromBody] UpdateDescriptionRequest request)
        {
            var command = new UpdateWorkspaceDescriptionCommand(workSpaceId, request.Description);
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{workSpaceId}/Background")]
        [SwaggerOperation(
            Summary = "Cập nhật ảnh nền Workspace",
            Description = "Thay đổi ảnh nền của Workspace."
        )]
        public async Task<IActionResult> UpdateBackground(Guid workSpaceId, [FromBody] UpdateBackgroundRequest request)
        {
            var command = new UpdateWorkspaceBackgroundCommand(workSpaceId, request.Background);
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{workSpaceId}/Type")]
        [SwaggerOperation(
            Summary = "Cập nhật loại Workspace",
            Description = "Cập nhật kiểu của Workspace (Private / Public / etc.)."
        )]
        public async Task<IActionResult> UpdateType(Guid workSpaceId, [FromBody] UpdateTypeRequest request)
        {
            var command = new UpdateWorkspaceTypeCommand(workSpaceId, request.Type);
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{workSpaceId}")]
        [SwaggerOperation(
            Summary = "Xoá Workspace",
            Description = "Chỉ Workspace Owner mới có quyền xoá Workspace."
        )]
        public async Task<IActionResult> Delete(Guid workSpaceId)
        {
            var result = await _mediator.Send(new DeleteWorkspaceCommand(workSpaceId));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }

    public class UpdateNameRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateDescriptionRequest
    {
        public string? Description { get; set; }
    }

    public class UpdateBackgroundRequest
    {
        public string? Background { get; set; }
    }

    public class UpdateTypeRequest
    {
        public WorkSpaceType Type { get; set; }
    }
}
