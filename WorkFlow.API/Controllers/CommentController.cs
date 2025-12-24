using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WorkFlow.Application.Features.Comments.Commands;
using WorkFlow.Application.Features.Comments.Queries;

namespace WorkFlow.API.Controllers
{
    [ApiController]
    [Route("Card")]
    [Authorize]
    public class CommentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CommentController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpGet("{cardId:guid}/Comments")]
        [SwaggerOperation(
            Summary = "Lấy danh sách comment của Card",
            Description = "Trả về toàn bộ comment của Card."
        )]
        public async Task<IActionResult> GetComments(Guid cardId)
        {
            var result = await _mediator.Send(new GetCommentsQuery(cardId));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }


        [HttpPost("{cardId:guid}/Comments")]
        [SwaggerOperation(
            Summary = "Thêm comment vào Card",
            Description = "Tạo comment mới cho Card."
        )]
        public async Task<IActionResult> AddComment(
            Guid cardId,
            [FromBody] CreateCommentCommand command)
        {
            command.SetCardId(cardId);
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }


        [HttpPut("Comments/{commentId:guid}")]
        [SwaggerOperation(
            Summary = "Chỉnh sửa comment",
            Description = "Chỉ người tạo comment mới được chỉnh sửa, chỉ được chỉnh sửa nếu thời gian tạo không quá hiện tại 36 giây."
        )]
        public async Task<IActionResult> UpdateComment(
            Guid commentId,
            [FromBody] UpdateCommentCommand command)
        {
            command.SetCommentId(commentId);
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }


        [HttpDelete("Comments/{commentId:guid}")]
        [SwaggerOperation(
            Summary = "Xoá comment",
            Description = "Chỉ người tạo comment hoặc Editor/Owner mới được xoá."
        )]
        public async Task<IActionResult> DeleteComment(Guid commentId)
        {
            var result = await _mediator.Send(new DeleteCommentCommand(commentId));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
