using MediatR;
using Microsoft.AspNetCore.Mvc;
using WorkFlow.Application.Features.InviteLinks.Commands.CreateInviteLink;
using WorkFlow.Application.Features.InviteLinks.Commands.RevokeInviteLink;
using WorkFlow.Application.Features.InviteLinks.Commands.VerifyInviteLink;
using WorkFlow.Application.Features.InviteLinks.Commands.JoinByInviteLink;
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

    [HttpPost("{token}/verify")]
    public async Task<IActionResult> Verify(string token)
    {
        var result = await _mediator.Send(new VerifyInviteLinkCommand(token));
        return Ok(result);
    }

    [HttpPost("{id:guid}/revoke")]
    public async Task<IActionResult> Revoke(Guid id)
    {
        await _mediator.Send(new RevokeInviteLinkCommand(id));
        return NoContent();
    }

        [HttpPost("{token}/join")]
        public async Task<IActionResult> Join(string token)
        {
            var result = await _mediator.Send(new JoinByInviteLinkCommand(token));
            return Ok(result);
        }
    }
}
