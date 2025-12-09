using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WorkFlow.Application.Features.Tasks.Commands;
using WorkFlow.Application.Features.Tasks.Queries;

namespace WorkFlow.API.Controllers
{
    [ApiController]
    [Route("tasks")]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TaskController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Tạo Task mới trong Card",
            Description = "Task đóng vai trò checklist group. Position tự động tăng."
        )]
        public async Task<IActionResult> Create([FromBody] CreateTaskRequest body)
        {
            var command = new CreateTaskCommand(body.CardId, body.Title);

            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        public class CreateTaskRequest
        {
            public Guid CardId { get; set; }
            public string Title { get; set; } = null!;
        }

        [HttpGet("/cards/{cardId:guid}/tasks")]
        [SwaggerOperation(
            Summary = "Lấy danh sách Task trong Card",
            Description = "Trả theo thứ tự Position."
        )]
        public async Task<IActionResult> GetTasks(Guid cardId)
        {
            var result = await _mediator.Send(new GetTasksQuery(cardId));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{taskId:guid}/title")]
        [SwaggerOperation(Summary = "Cập nhật tiêu đề Task")]
        public async Task<IActionResult> UpdateTitle(Guid taskId, [FromBody] string title)
        {
            var result = await _mediator.Send(new UpdateTaskTitleCommand(taskId, title));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{taskId:guid}/move")]
        [SwaggerOperation(
            Summary = "Di chuyển Task (reorder)",
            Description = "Move Task sang vị trí khác trong cùng Card."
        )]
        public async Task<IActionResult> Move(Guid taskId, [FromBody] MoveTaskRequest body)
        {
            var command = new MoveTaskCommand(
                TaskId: taskId,
                NewPosition: body.NewPosition
            );

            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        public class MoveTaskRequest
        {
            public int NewPosition { get; set; }
        }

        [HttpDelete("{taskId:guid}")]
        [SwaggerOperation(
            Summary = "Xóa Task",
            Description = "Task bị xóa sẽ tự động xóa toàn bộ SubTask."
        )]
        public async Task<IActionResult> Delete(Guid taskId)
        {
            var result = await _mediator.Send(new DeleteTaskCommand(taskId));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}