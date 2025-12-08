using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WorkFlow.Application.Features.Boards.Commands;
using WorkFlow.Application.Features.WorkSpaces.Commands;

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
            return result.IsSuccess
                ? Ok(result)
                : BadRequest(result);
        }

        [HttpPut("{boardId:guid}/Title")]
        [SwaggerOperation(
            Summary = "Cập nhật tiêu đề Board",
            Description = "Cập nhật tiêu đề của Board. Yêu cầu người dùng có quyền Editor hoặc Owner trong Board."
        )]
        public async Task<IActionResult> UpdateTitle(Guid boardId, [FromBody] string title)
        {
            var result = await _mediator.Send(new UpdateBoardTitleCommand(boardId, title));

            return result.IsSuccess
                ? Ok(result)
                : BadRequest(result);
        }
    }
}
