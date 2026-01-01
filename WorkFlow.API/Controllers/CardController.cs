using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WorkFlow.Application.Features.Boards.Commands;
using WorkFlow.Application.Features.Cards.Commands;
using WorkFlow.Application.Features.Cards.Queries;

namespace WorkFlow.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class CardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Tạo mới một Card trong List",
            Description = """
                Tạo card mới, position sẽ được tự động set ở cuối list.
                Yêu cầu quyền Editor hoặc Owner của Board chứa List.
                """
        )]
        public async Task<IActionResult> Create([FromBody] CreateCardCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{cardId:guid}")]
        [SwaggerOperation(
            Summary = "Lấy chi tiết Card",
            Description = """
                Trả về thông tin chi tiết của 1 Card, bao gồm:
                - Basic info
                - Labels, Dates
                - Assignees
                - Attachments
                """
        )]
        public async Task<IActionResult> GetDetail(Guid cardId)
        {
            var result = await _mediator.Send(new GetCardDetailQuery(cardId));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{cardId:guid}/Title")]
        [SwaggerOperation(Summary = "Cập nhật tiêu đề Card")]
        public async Task<IActionResult> UpdateTitle(Guid cardId, [FromBody] string title)
        {
            var result = await _mediator.Send(new UpdateCardTitleCommand(cardId, title));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{cardId:guid}/Description")]
        [SwaggerOperation(Summary = "Cập nhật mô tả của Card")]
        public async Task<IActionResult> UpdateDescription(Guid cardId, [FromBody] string description)
        {
            var result = await _mediator.Send(new UpdateCardDescriptionCommand(cardId, description));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }


        [HttpPut("{cardId:guid}/Labels")]
        [SwaggerOperation(Summary = "Cập nhật nhãn (labels) cho Card")]
        public async Task<IActionResult> UpdateLabels(Guid cardId, [FromBody] int[] labels)
        {
            var command = new UpdateCardLabelsCommand(cardId, labels);

            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{cardId:guid}/Dates")]
        [SwaggerOperation(
            Summary = "Update StartDate, DueDate, Reminder",
            Description = "Nhập toàn bộ fields cần update trong body."
        )]
        public async Task<IActionResult> UpdateDates(
            Guid cardId,
            [FromBody] UpdateCardDatesRequest body)
        {
            var command = new UpdateCardDatesCommand(
                CardId: cardId,
                StartDate: body.StartDate,
                DueDate: body.DueDate,
                ReminderEnabled: body.ReminderEnabled,
                ReminderBeforeMinutes: body.ReminderBeforeMinutes
            );

            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{cardId:guid}/Move")]
        [SwaggerOperation(Summary = "Di chuyển card (drag & drop)")]
        public async Task<IActionResult> Move(Guid cardId, [FromBody] MoveCardCommand body)
        {
            var command = body with { CardId = cardId };
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{cardId:guid}")]
        [SwaggerOperation(Summary = "Xoá card (soft-delete)")]
        public async Task<IActionResult> Delete(Guid cardId)
        {
            var result = await _mediator.Send(new DeleteCardCommand(cardId));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{cardId:guid}/Restore")]
        [SwaggerOperation(
            Summary = "Khôi phục card đã bị xoá (redo soft-delete)",
            Description = """
                Khôi phục lại card đã bị soft-delete.
                Yêu cầu quyền Editor hoặc Owner của Board.
                """
        )]
        public async Task<IActionResult> Restore(Guid cardId)
        {
            var result = await _mediator.Send(new RestoreCardCommand(cardId));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

    }
    public class UpdateCardDatesRequest
    {
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public bool ReminderEnabled { get; set; }
        public int? ReminderBeforeMinutes { get; set; }
    }
}
