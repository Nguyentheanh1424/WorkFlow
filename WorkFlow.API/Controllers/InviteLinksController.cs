using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WorkFlow.Application.Features.InviteLinks.Commands;
using WorkFlow.Application.Features.InviteLinks.Queries.WorkFlow.Application.Features.InviteLinks.Queries;
using WorkFlow.Domain.Enums;

namespace WorkFlow.API.Controllers
{

    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class InviteLinkController : ControllerBase
    {
        private readonly IMediator _mediator;

        public InviteLinkController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Tạo Invite Link",
            Description = """
                Tạo quyền mời tạm thời cho Workspace hoặc Board.
                - Chỉ Owner / người có quyền mời mới được tạo
                - Invite Link luôn ở trạng thái Active
                - Không cấp quyền vượt quá Owner
                """
        )]
        public async Task<IActionResult> Create([FromBody] CreateInviteLinkCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Danh sách Invite Link",
            Description = """
                Dùng để quản trị Invite Link của Workspace hoặc Board.
                - Chỉ Owner hoặc người được phân quyền
                - Bao gồm Active / Revoked / Expired
                - Không trả về token hoặc rule nội bộ
                """
        )]
        public async Task<IActionResult> GetAll(
            [FromQuery] InviteLinkType targetType,
            [FromQuery] Guid targetId)
        {
            var query = new GetInviteLinksQuery(targetType, targetId);
            var result = await _mediator.Send(query);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{inviteLinkId:guid}/Revoke")]
        [SwaggerOperation(
            Summary = "Thu hồi Invite Link",
            Description = """
                Thu hồi vĩnh viễn quyền mời.
                - Không thể hoàn tác
                - Không ảnh hưởng user đã join trước đó
                """
        )]
        public async Task<IActionResult> Revoke(Guid inviteLinkId)
        {
            var result = await _mediator.Send(new RevokeInviteLinkCommand(inviteLinkId));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("Join")]
        [SwaggerOperation(
            Summary = "Join bằng Invite Link",
            Description = """
                Validate + Join trong một intent duy nhất.
                - Không có validate trước
                - Không preview Invite Link
                - Domain quyết định hợp lệ hay không
                """
        )]
        public async Task<IActionResult> Join([FromBody] JoinByInviteLinkCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
