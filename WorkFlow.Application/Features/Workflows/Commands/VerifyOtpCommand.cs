using MediatR;
using WorkFlow.Application.Features.Workflows.Dtos;

namespace WorkFlow.Application.Features.Workflows.Commands
{
    public record VerifyOtpCommand(OtpDto Dto) : IRequest<bool>;
}
