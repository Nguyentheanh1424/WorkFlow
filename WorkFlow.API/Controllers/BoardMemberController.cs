using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        [HttpGet("{boardId:guid}/members")]
        public async Task<IActionResult> GetBoardMembers(Guid boardId)
        {
            var result = await _mediator.Send(new GetBoardMembersQuery(boardId));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{boardId:guid}/members")]
        public async Task<IActionResult> AddBoardMember(Guid boardId, [FromBody] AddMemberBody body)
        {
            var command = new AddBoardMemberCommand(boardId, body.UserId, body.Role);
            var result = await _mediator.Send(command);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        public class AddMemberBody
        {
            public Guid UserId { get; set; }
            public BoardRole Role { get; set; }
        }

        [HttpDelete("{boardId:guid}/members/{userId:guid}")]
        public async Task<IActionResult> RemoveBoardMember(Guid boardId, Guid userId)
        {
            var result = await _mediator.Send(new RemoveBoardMemberCommand(boardId, userId));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
