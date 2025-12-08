using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WorkFlow.Application.Features.BoardMembers.Commands;
using WorkFlow.Application.Features.BoardMembers.Queries;
using WorkFlow.Domain.Enums;

namespace WorkFlow.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class BoardMemberController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BoardMemberController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{boardId:guid}/Members")]
        [SwaggerOperation(
            Summary = "Lấy danh sách thành viên của Board",
            Description = "Trả về danh sách UserId, Name, Role và JoinedAt của thành viên trong Board. " +
                          "Yêu cầu quyền Viewer hoặc Workspace Member trở lên."
        )]
        public async Task<IActionResult> GetBoardMembers(Guid boardId)
        {
            var result = await _mediator.Send(new GetBoardMembersQuery(boardId));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{boardId:guid}/Members")]
        [SwaggerOperation(
            Summary = "Thêm thành viên vào Board",
            Description = "Thêm người dùng vào Board với role chỉ định. " +
                          "Workspace Owner/Admin hoặc Board Owner/Editor có quyền thêm. " +
                          "Không thể thêm người có role cao hơn mình."
        )]
        public async Task<IActionResult> AddBoardMember(Guid boardId, [FromBody] AddBoardMemberBody body)
        {
            var command = new AddBoardMemberCommand(boardId, body.UserId, body.Role);
            var result = await _mediator.Send(command);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        public class AddBoardMemberBody
        {
            public Guid UserId { get; set; }
            public BoardRole Role { get; set; }
        }

        [HttpPut("{boardId:guid}/Members/{userId:guid}")]
        [SwaggerOperation(
            Summary = "Cập nhật quyền của thành viên trong Board",
            Description = "Cập nhật role của một thành viên trong Board. " +
                          "Workspace Owner/Admin hoặc Board Owner có quyền chỉnh sửa. " +
                          "Không thể chỉnh sửa role của thành viên có quyền cao hơn mình hoặc Owner cuối cùng."
        )]
        public async Task<IActionResult> UpdateBoardRole(Guid boardId, Guid userId, [FromBody] UpdateBoardRoleBody body)
        {
            var command = new UpdateBoardRoleCommand
            {
                BoardId = boardId,
                UserId = userId,
                NewRole = body.Role
            };

            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        public class UpdateBoardRoleBody
        {
            public BoardRole Role { get; set; }
        }

        [HttpDelete("{boardId:guid}/Members/{userId:guid}")]
        [SwaggerOperation(
            Summary = "Xoá thành viên khỏi Board",
            Description = "Xoá thành viên khỏi Board. " +
                          "Workspace Owner/Admin hoặc Board Owner có quyền xoá. " +
                          "Không thể xoá Owner cuối cùng. " +
                          "User có thể tự rời board trừ khi là Owner cuối cùng."
        )]
        public async Task<IActionResult> RemoveBoardMember(Guid boardId, Guid userId)
        {
            var result = await _mediator.Send(new RemoveBoardMemberCommand(boardId, userId));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
