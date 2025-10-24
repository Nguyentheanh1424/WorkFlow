using FluentValidation;
using MediatR;
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Workflows.Commands
{
    public record CreateWorkflowCommand(string Name, string Description) : IRequest<Guid>;

    public class CreateWorkflowCommandValidator : AbstractValidator<CreateWorkflowCommand>
    {
        public CreateWorkflowCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên workflow không được để trống.")
                .MaximumLength(100).WithMessage("Tên workflow không được vượt quá 100 ký tự.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Mô tả workflow không được vượt quá 500 ký tự.");
        }
    }

    public class CreateWorkflowCommandHandler : IRequestHandler<CreateWorkflowCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Workflow, Guid> _workflowRepository;

        public CreateWorkflowCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _workflowRepository = _unitOfWork.GetRepository<Workflow, Guid>();
        }
        public async Task<Guid> Handle(CreateWorkflowCommand request, CancellationToken cancellationToken)
        {
            var entiry = new Workflow(request.Name, request.Description);

            await _workflowRepository.AddAsync(entiry);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return entiry.Id;
        }
    }
}
