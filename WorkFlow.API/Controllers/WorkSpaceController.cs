using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkFlow.Application.Features.WorkSpaces.Commands;
using WorkFlow.Application.Features.WorkSpaces.Queries;

namespace WorkFlow.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class WorkSpaceController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WorkSpaceController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateWorkspaceCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess
                ? Ok(result)
                : BadRequest(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search)
        {
            var result = await _mediator.Send(new GetWorkspacesQuery(search));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetWorkspaceDetailQuery(id));
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpPut("{id}/name")]
        public async Task<IActionResult> UpdateName(Guid id, [FromBody] UpdateNameRequest request)
        {
            var command = new UpdateWorkspaceNameCommand(id, request.Name);
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{id}/description")]
        public async Task<IActionResult> UpdateDescription(Guid id, [FromBody] UpdateDescriptionRequest request)
        {
            var command = new UpdateWorkspaceDescriptionCommand(id, request.Description);
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{id}/background")]
        public async Task<IActionResult> UpdateBackground(Guid id, [FromBody] UpdateBackgroundRequest request)
        {
            var command = new UpdateWorkspaceBackgroundCommand(id, request.Background);
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{id}/type")]
        public async Task<IActionResult> UpdateType(Guid id, [FromBody] UpdateTypeRequest request)
        {
            var command = new UpdateWorkspaceTypeCommand(id, request.Type);
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _mediator.Send(new DeleteWorkspaceCommand(id));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
