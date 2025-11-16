using MediatR;
using WorkFlow.Application.Features.Workflows.Dtos;

namespace WorkFlow.Application.Features.Workflows.Commands
{
    public record RegisterUserCommand(RegisterUserDto Dto) : IRequest<bool>;
}
