using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WorkFlow.Application.Features.Boards.Commands;
using WorkFlow.Application.Features.Boards.Queries;
using WorkFlow.Domain.Enums;

namespace WorkFlow.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class BoardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BoardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("Create")]
        [SwaggerOperation(
            Summary = "Tạo mới một Board",
            Description = "Tạo mới một Board trong Workspace. Người dùng phải có quyền tạo Board trong Workspace."
        )]
        public async Task<IActionResult> Create([FromBody] CreateBoardCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet]
        [SwaggerOperation(
             Summary = "Lấy danh sách Board trong Workspace",
             Description = """
                Lọc danh sách Board theo nhiều tiêu chí:
                - keyword
                - visibility (Private / Public / Protected)
                - pinned
                - includeArchived
                - sort (CreatedAt / UpdatedAt / Title)
                - role (Viewer / Editor / Owner)
                """
        )]
        public async Task<IActionResult> GetBoards([FromQuery] GetBoardsQuery query)
        {
            var result = await _mediator.Send(query);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{boardId:guid}")]
        [SwaggerOperation(
            Summary = "Lấy thông tin cơ bản của Board",
            Description = "Trả về thông tin Board mà không bao gồm Lists/Cards/Members."
        )]
        public async Task<IActionResult> GetDetail(Guid boardId)
        {
            var result = await _mediator.Send(new GetBoardDetailQuery(boardId));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{boardId:guid}/Full")]
        [SwaggerOperation(
            Summary = "Lấy toàn bộ dữ liệu Board",
            Description = """
                Dùng khi FE mở Board UI.
                Trả về:
                - Board
                - Lists
                - Cards
                - Board Members
                - Role của user hiện tại
                """
        )]
        public async Task<IActionResult> GetFullDetail(Guid boardId)
        {
            var result = await _mediator.Send(new GetBoardFullDetailQuery(boardId));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{boardId:guid}/Title")]
        [SwaggerOperation(Summary = "Cập nhật tiêu đề Board")]
        public async Task<IActionResult> UpdateTitle(Guid boardId, [FromBody] string title)
        {
            var result = await _mediator.Send(new UpdateBoardTitleCommand(boardId, title));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{boardId:guid}/Description")]
        [SwaggerOperation(Summary = "Cập nhật mô tả Board")]
        public async Task<IActionResult> UpdateDescription(Guid boardId, [FromBody] string description)
        {
            var result = await _mediator.Send(new UpdateBoardDescriptionCommand(boardId, description));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{boardId:guid}/Background")]
        [SwaggerOperation(Summary = "Cập nhật ảnh nền Board")]
        public async Task<IActionResult> UpdateBackground(Guid boardId, [FromBody] string background)
        {
            var result = await _mediator.Send(new UpdateBoardBackgroundCommand(boardId, background));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{boardId:guid}/Visibility")]
        [SwaggerOperation(Summary = "Thay đổi chế độ hiển thị (Public / Private)")]
        public async Task<IActionResult> UpdateVisibility(Guid boardId, [FromBody] VisibilityBoard visibility)
        {
            var result = await _mediator.Send(new UpdateBoardVisibilityCommand(boardId, visibility));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{boardId:guid}/Pinned")]
        [SwaggerOperation(Summary = "Ghim hoặc bỏ ghim Board")]
        public async Task<IActionResult> UpdatePinned(Guid boardId, [FromBody] bool pinned)
        {
            var result = await _mediator.Send(new UpdateBoardPinnedCommand(boardId, pinned));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{boardId:guid}/Archive")]
        [SwaggerOperation(Summary = "Lưu trữ Board")]
        public async Task<IActionResult> Archive(Guid boardId)
        {
            var result = await _mediator.Send(new ArchiveBoardCommand(boardId));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{boardId:guid}/Restore")]
        [SwaggerOperation(Summary = "Bỏ lưu trữ Board")]
        public async Task<IActionResult> Restore(Guid boardId)
        {
            var result = await _mediator.Send(new RestoreBoardCommand(boardId));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{boardId:guid}/Duplicate")]
        [SwaggerOperation(
            Summary = "Tạo bản sao của Board",
            Description = """
                Tạo một Board mới bằng cách sao chép từ Board hiện tại.
                Hỗ trợ các tùy chọn:
                - copyLists: Sao chép toàn bộ Lists
                - copyCards: Sao chép toàn bộ Cards tương ứng với Lists (yêu cầu copyLists = true)

                Quy tắc hoạt động:
                - Nếu copyLists = false → Board mới sẽ không có list.
                - Nếu copyLists = true và copyCards = false → Lists sẽ rỗng.
                - Nếu copyLists = true và copyCards = true → Sao chép toàn bộ Lists và Cards.
                - Không thể copyCards = true nếu copyLists = false.
                - Board Members không được copy. Người tạo là Owner duy nhất.

                Trả về BoardDto của Board mới.
                """
        )]
        public async Task<IActionResult> Duplicate(Guid boardId, [FromBody] DuplicateBoardRequest body)
        {
            var command = new DuplicateBoardCommand(
                BoardId: boardId,
                CopyLists: body.CopyLists,
                CopyCards: body.CopyCards
            );

            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }



        [HttpDelete("{boardId:guid}")]
        [SwaggerOperation(Summary = "Xóa Board")]
        public async Task<IActionResult> Delete(Guid boardId)
        {
            var result = await _mediator.Send(new DeleteBoardCommand(boardId));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }

    public class DuplicateBoardRequest
    {
        public bool CopyLists { get; set; } = true;
        public bool CopyCards { get; set; } = true;
    }

}
