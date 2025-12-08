using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WorkFlow.Application.Features.Lists.Commands;
using WorkFlow.Application.Features.Lists.Queries;

namespace WorkFlow.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class ListController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ListController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Tạo List mới trong Board",
            Description = """
                Tạo mới một List trong Board.
                - position sẽ được tự động tính theo cuối danh sách.
                - yêu cầu quyền Editor hoặc Owner của Board.
                """
        )]
        public async Task<IActionResult> Create([FromBody] CreateListCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }


        [HttpGet("/Boards/{boardId:guid}/Lists")]
        [SwaggerOperation(
            Summary = "Lấy danh sách List trong Board",
            Description = """
                Trả về toàn bộ Lists trong Board, sắp xếp theo Position.
                Bao gồm cả trạng thái archived nếu có.
                """
        )]
        public async Task<IActionResult> GetLists(Guid boardId)
        {
            var result = await _mediator.Send(new GetListsQuery(boardId));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }


        [HttpPut("{listId:guid}/Title")]
        [SwaggerOperation(
            Summary = "Đổi tên List",
            Description = "Yêu cầu Editor hoặc Owner trong Board."
        )]
        public async Task<IActionResult> UpdateTitle(Guid listId, [FromBody] string title)
        {
            var result = await _mediator.Send(new UpdateListTitleCommand(listId, title));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }


        [HttpPut("{listId:guid}/Position")]
        [SwaggerOperation(
            Summary = "Thay đổi vị trí List",
            Description = """
                Dùng cho thao tác kéo thả (drag/drop). 
                BE sẽ cập nhật Position của List và sắp xếp lại thứ tự.
                """
        )]
        public async Task<IActionResult> UpdatePosition(Guid listId, [FromBody] int position)
        {
            var result = await _mediator.Send(new UpdateListPositionCommand(listId, position));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }


        public class MoveListRequest
        {
            public Guid TargetBoardId { get; set; }
        }

        [HttpPut("{listId:guid}/Move")]
        [SwaggerOperation(
            Summary = "Di chuyển List sang Board khác",
            Description = """
                Di chuyển toàn bộ List sang một Board khác.
                - Cards trong List cũng di chuyển theo.
                - Yêu cầu quyền Editor trên Board hiện tại và Board mới.
                """
        )]
        // Khó đấy =))
        public async Task<IActionResult> Move(Guid listId, [FromBody] MoveListRequest body)
        {
            var command = new MoveListToBoardCommand(listId, body.TargetBoardId);
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }


        [HttpPut("{listId:guid}/Archive")]
        [SwaggerOperation(
            Summary = "Archive List",
            Description = "Ẩn List khỏi Board nhưng không xoá dữ liệu."
        )]
        public async Task<IActionResult> Archive(Guid listId)
        {
            var result = await _mediator.Send(new ArchiveListCommand(listId));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }


        [HttpPut("{listId:guid}/Unarchive")]
        [SwaggerOperation(
            Summary = "Khôi phục List",
            Description = "Khôi phục List đã bị archive."
        )]
        public async Task<IActionResult> Unarchive(Guid listId)
        {
            var result = await _mediator.Send(new UnarchiveListCommand(listId));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        public class CloneListRequest
        {
            public bool CopyCards { get; set; } = true;
        }

        [HttpPost("{listId:guid}/Clone")]
        [SwaggerOperation(
            Summary = "Clone một List",
            Description = """
                Tạo bản sao của List hiện tại.
                - CopyCards = true → copy toàn bộ cards trong list.
                - List mới sẽ nằm ở cuối Board.
                """
        )]
        public async Task<IActionResult> Clone(Guid listId, [FromBody] CloneListRequest body)
        {
            var command = new CloneListCommand(listId, body.CopyCards);
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }


        public class MoveCardsRequest
        {
            public Guid TargetListId { get; set; }
        }

        [HttpPost("{sourceListId:guid}/Move-Cards")]
        [SwaggerOperation(
            Summary = "Di chuyển toàn bộ Cards sang List khác",
            Description = """
                Chuyển toàn bộ Cards từ List hiện tại sang List mục tiêu.
                Thứ tự Cards được giữ nguyên.
                """
        )]
        public async Task<IActionResult> MoveCards(Guid sourceListId, [FromBody] MoveCardsRequest body)
        {
            var command = new MoveAllCardsFromListCommand(sourceListId, body.TargetListId);
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
