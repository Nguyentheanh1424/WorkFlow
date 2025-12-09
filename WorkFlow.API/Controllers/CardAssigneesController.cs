using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WorkFlow.Application.Features.CardAssignees.Commands;
using WorkFlow.Application.Features.CardAssignees.Queries;

namespace WorkFlow.API.Controllers
{
    [ApiController]
    [Route("Cards/{cardId:guid}/Assignees")]
    [Authorize]
    public class CardAssigneesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CardAssigneesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Lấy danh sách người được gán vào card",
            Description = "Yêu cầu quyền Viewer của board chứa card."
        )]
        public async Task<IActionResult> Get(Guid cardId)
        {
            var result = await _mediator.Send(new GetCardAssigneesQuery(cardId));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Gán người dùng vào card",
            Description = "Yêu cầu quyền Editor của board."
        )]
        public async Task<IActionResult> Add(Guid cardId, [FromBody] AddAssigneeRequest body)
        {
            var command = new AddCardAssigneeCommand(
                CardId: cardId,
                UserId: body.UserId
            );

            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        public class AddAssigneeRequest
        {
            public Guid UserId { get; set; }
        }

        [HttpDelete("{userId:guid}")]
        [SwaggerOperation(
            Summary = "Xóa người dùng khỏi card",
            Description = "Yêu cầu quyền Editor của board."
        )]
        public async Task<IActionResult> Remove(Guid cardId, Guid userId)
        {
            var result = await _mediator.Send(
                new RemoveCardAssigneeCommand(cardId, userId)
            );

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
