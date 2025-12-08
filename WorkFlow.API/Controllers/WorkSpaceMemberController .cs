using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WorkFlow.Application.Features.WorkSpaceMembers.Commands;
using WorkFlow.Application.Features.WorkSpaceMembers.Queries;
using WorkFlow.Domain.Enums;

namespace WorkFlow.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class WorkSpaceMemberController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WorkSpaceMemberController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{workspaceId:guid}/Members")]
        [SwaggerOperation(
            Summary = "Lấy danh sách thành viên Workspace",
            Description = "Trả danh sách UserId, Name, Role và JoinedAt của tất cả thành viên. " +
                          "Chỉ người thuộc Workspace mới xem được."
        )]
        public async Task<IActionResult> GetWorkspaceMembers(Guid workspaceId)
        {
            var result = await _mediator.Send(new GetWorkSpaceMembersQuery(workspaceId));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{workspaceId:guid}/Members")]
        [SwaggerOperation(
            Summary = "Thêm thành viên vào Workspace",
            Description = "Workspace Owner hoặc Admin có thể thêm thành viên mới. " +
                          "Không thể thêm người với quyền cao hơn quyền hiện tại."
        )]
        public async Task<IActionResult> AddWorkspaceMember(Guid workspaceId, [FromBody] AddWorkSpaceMemberBody body)
        {
            var command = new AddWorkSpaceMemberCommand
            {
                WorkspaceId = workspaceId,
                UserId = body.UserId,
                Role = body.Role
            };

            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        public class AddWorkSpaceMemberBody
        {
            public Guid UserId { get; set; }
            public WorkSpaceRole Role { get; set; }
        }

        [HttpPut("{workspaceId:guid}/Members/{userId:guid}")]
        [SwaggerOperation(
            Summary = "Cập nhật quyền của thành viên Workspace",
            Description = "Workspace Owner hoặc Admin được quyền cập nhật role. " +
                          "Không thể sửa role của Owner cuối cùng hoặc thành viên có role cao hơn."
        )]
        public async Task<IActionResult> UpdateRole(Guid workspaceId, Guid userId, [FromBody] UpdateWorkSpaceRoleBody body)
        {
            var command = new UpdateWorkSpaceRoleCommand
            {
                WorkspaceId = workspaceId,
                UserId = userId,
                NewRole = body.Role
            };

            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        public class UpdateWorkSpaceRoleBody
        {
            public WorkSpaceRole Role { get; set; }
        }

        [HttpDelete("{workspaceId:guid}/Members/{userId:guid}")]
        [SwaggerOperation(
            Summary = "Xoá thành viên khỏi Workspace",
            Description =
                "Workspace Owner hoặc Admin có thể xoá thành viên khác. " +
                "Không thể xoá Owner cuối cùng. " +
                "User có thể tự rời Workspace trừ khi là Owner cuối cùng."
        )]
        public async Task<IActionResult> RemoveWorkspaceMember(Guid workspaceId, Guid userId)
        {
            var command = new RemoveWorkSpaceMemberCommand
            {
                WorkspaceId = workspaceId,
                TargetUserId = userId
            };

            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
