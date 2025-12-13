using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WorkFlow.Application.Features.SubTasks.Commands;
using WorkFlow.Application.Features.SubTasks.Queries;
using WorkFlow.Domain.Enums;

namespace WorkFlow.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class SubTaskController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SubTaskController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Tạo SubTask mới trong Task",
            Description = "Position tự động tăng theo số lượng SubTask hiện có."
        )]
        public async Task<IActionResult> Create([FromBody] CreateSubTaskRequest body)
        {
            var command = new CreateSubTaskCommand(
                TaskId: body.TaskId,
                Title: body.Title
            );

            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        public class CreateSubTaskRequest
        {
            public Guid TaskId { get; set; }
            public string Title { get; set; } = null!;
        }

        [HttpGet("/Tasks/{taskId:guid}/Subtasks")]
        [SwaggerOperation(
            Summary = "Lấy danh sách SubTask theo Task",
            Description = "Trả về danh sách sắp xếp theo Position."
        )]
        public async Task<IActionResult> GetByTask(Guid taskId)
        {
            var result = await _mediator.Send(new GetSubTasksQuery(taskId));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{subTaskId:guid}/Title")]
        [SwaggerOperation(Summary = "Cập nhật tiêu đề SubTask")]
        public async Task<IActionResult> UpdateTitle(Guid subTaskId, [FromBody] string title)
        {
            var result = await _mediator.Send(new UpdateSubTaskTitleCommand(subTaskId, title));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{subTaskId:guid}/Status")]
        [SwaggerOperation(Summary = "Cập nhật trạng thái SubTask")]
        public async Task<IActionResult> UpdateStatus(Guid subTaskId, [FromBody] UpdateSubTaskStatusRequest body)
        {
            var command = new UpdateSubTaskStatusCommand(subTaskId, body.Status);
            var result = await _mediator.Send(command);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        public class UpdateSubTaskStatusRequest
        {
            public JobStatus Status { get; set; }
        }

        [HttpPut("{subTaskId:guid}/Dates")]
        [SwaggerOperation(Summary = "Cập nhật DueDate và Reminder cho SubTask")]
        public async Task<IActionResult> UpdateDates(Guid subTaskId, [FromBody] UpdateSubTaskDatesRequest body)
        {
            var command = new UpdateSubTaskDatesCommand(
                SubTaskId: subTaskId,
                DueDate: body.DueDate,
                ReminderEnabled: body.ReminderEnabled,
                ReminderBeforeMinutes: body.ReminderBeforeMinutes
            );

            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        public class UpdateSubTaskDatesRequest
        {
            public DateTime? DueDate { get; set; }
            public bool ReminderEnabled { get; set; }
            public int? ReminderBeforeMinutes { get; set; }
        }

        [HttpPut("{subTaskId:guid}/Move")]
        [SwaggerOperation(
            Summary = "Di chuyển SubTask (reorder)",
            Description = "Di chuyển vị trí SubTask trong cùng Task."
        )]
        public async Task<IActionResult> Move(Guid subTaskId, [FromBody] MoveSubTaskRequest body)
        {
            var command = new MoveSubTaskCommand(
                SubTaskId: subTaskId,
                NewPosition: body.NewPosition
            );

            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        public class MoveSubTaskRequest
        {
            public int NewPosition { get; set; }
        }

        [HttpDelete("{subTaskId:guid}")]
        [SwaggerOperation(
            Summary = "Xóa SubTask",
            Description = "Tự động xóa SubTaskAssignees liên quan."
        )]
        public async Task<IActionResult> Delete(Guid subTaskId)
        {
            var result = await _mediator.Send(new DeleteSubTaskCommand(subTaskId));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
