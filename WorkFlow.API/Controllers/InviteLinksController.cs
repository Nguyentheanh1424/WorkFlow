using MediatR;
using Microsoft.AspNetCore.Mvc;
using WorkFlow.Application.Features.InviteLinks.Commands;
using WorkFlow.Application.Features.InviteLinks.Dtos;
using WorkFlow.Application.Features.InviteLinks.Queries;

namespace WorkFlow.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InviteLinkController : ControllerBase
    {
        private readonly IMediator _mediator;

        public InviteLinkController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("{create}")]
        public async Task<IActionResult> Create([FromBody] CreateInviteLinkDto request)
        {
            var result = await _mediator.Send(new CreateInviteLinkCommand(request));
            return Ok(result);
        }

        [HttpGet("{token}")]
        public async Task<IActionResult> GetInfo(string token)
        {
            var result = await _mediator.Send(new GetInviteLinkQuery(token));
            return Ok(result);
        }

        [HttpPost("{token}/Verify")]
        public async Task<IActionResult> Verify(string token)
        {
            var result = await _mediator.Send(new VerifyInviteLinkCommand(token));
            return Ok(result);
        }

        [HttpPost("{tagetId:guid}/Revoke")]
        public async Task<IActionResult> Revoke(Guid tagetId)
        {
            var result = await _mediator.Send(new RevokeInviteLinkCommand(tagetId));
            return Ok(result);
        }

        [HttpPost("{token}/Join")]
        public async Task<IActionResult> Join(string token)
        {
            var result = await _mediator.Send(new JoinByInviteLinkCommand(token));
            return Ok(result);
        }
    }
}
