using MediatR;
using Microsoft.AspNetCore.Mvc;
using WorkFlow.Application.Features.Workflows.Commands;
using WorkFlow.Application.Features.Workflows.Queries;

namespace WorkFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowsController
    {
        private readonly IMediator _mediator;

        public WorkflowsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var query = new GetWorkflowByIdQuery(id);
            var result = await _mediator.Send(query);

            return new OkObjectResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateWorkflowCommand command)
        {
            var id = await _mediator.Send(command);
            return new OkObjectResult(new { Id = id });
        }
    }
}
