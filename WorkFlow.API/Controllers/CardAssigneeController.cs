//using MediatR;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Swashbuckle.AspNetCore.Annotations;

//namespace WorkFlow.API.Controllers
//{
//    [ApiController]
//    [Route("cards/{cardId:guid}/assignees")]
//    [Authorize]
//    public class CardAssigneeController : ControllerBase
//    {
//        private readonly IMediator _mediator;

//        public CardAssigneeController(IMediator mediator)
//        {
//            _mediator = mediator;
//        }

//        // GET: cards/{cardId}/assignees
//        [HttpGet]
//        [SwaggerOperation(
//            Summary = "Lấy danh sách assignees của Card",
//            Description = "Trả về danh sách user được gán vào Card."
//        )]
//        public async Task<IActionResult> GetAssignees(Guid cardId)
//        {
//            var result = await _mediator.Send(new GetCardAssigneesQuery(cardId));
//            return result.IsSuccess ? Ok(result) : BadRequest(result);
//        }

//        // POST: cards/{cardId}/assignees
//        [HttpPost]
//        [SwaggerOperation(
//            Summary = "Gán người dùng vào Card",
//            Description = "Thêm một user vào danh sách assignees của Card."
//        )]
//        public async Task<IActionResult> AddAssignee(Guid cardId, [FromBody] AddCardAssigneeCommand command)
//        {
//            command.SetCardId(cardId);
//            var result = await _mediator.Send(command);
//            return result.IsSuccess ? Ok(result) : BadRequest(result);
//        }

//        // DELETE: cards/{cardId}/assignees/{userId}
//        [HttpDelete("{userId:guid}")]
//        [SwaggerOperation(
//            Summary = "Gỡ người dùng khỏi Card",
//            Description = "Xóa một user khỏi danh sách assignees của Card."
//        )]
//        public async Task<IActionResult> RemoveAssignee(Guid cardId, Guid userId)
//        {
//            var result = await _mediator.Send(new RemoveCardAssigneeCommand(cardId, userId));
//            return result.IsSuccess ? Ok(result) : BadRequest(result);
//        }
//    }
//}
