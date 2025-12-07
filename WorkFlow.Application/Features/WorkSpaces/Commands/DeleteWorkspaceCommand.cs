using MediatR;
using FluentValidation;
using WorkFlow.Domain.Common;

namespace WorkFlow.Application.Features.WorkSpaces.Commands
{
    public record DeleteWorkspaceCommand(Guid Id) : IRequest<Result<bool>>;

    public class DeleteWorkspaceCommandValidator : AbstractValidator<DeleteWorkspaceCommand>
    {
        public DeleteWorkspaceCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
