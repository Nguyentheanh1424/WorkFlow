using FluentValidation;
using MediatR;
using AutoMapper;
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Application.Features.WorkSpaces.Dtos;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Common;

namespace WorkFlow.Application.Features.WorkSpaces.Commands
{
    public record UpdateWorkspaceBackgroundCommand(Guid Id, string? Background) : IRequest<Result<WorkSpaceDto>>;

    public class UpdateWorkspaceBackgroundCommandValidator : AbstractValidator<UpdateWorkspaceBackgroundCommand>
    {
        public UpdateWorkspaceBackgroundCommandValidator()
        {
            RuleFor(x => x.Background)
                .MaximumLength(500).WithMessage("Background không được vượt quá 500 ký tự.");
        }
    }

    public class UpdateWorkspaceBackgroundCommandHandler : IRequestHandler<UpdateWorkspaceBackgroundCommand, Result<WorkSpaceDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<WorkSpace, Guid> _repository;
        private readonly IMapper _mapper;

        public UpdateWorkspaceBackgroundCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _repository = _unitOfWork.GetRepository<WorkSpace, Guid>();
            _mapper = mapper;
        }

        public async Task<Result<WorkSpaceDto>> Handle(UpdateWorkspaceBackgroundCommand request, CancellationToken cancellationToken)
        {
            var workspace = await _repository.GetByIdAsync(request.Id);
            if (workspace == null)
                return Result<WorkSpaceDto>.Failure("Workspace không tồn tại.");

            workspace.UpdateBackground(request.Background);

            await _unitOfWork.SaveChangesAsync();

            return Result<WorkSpaceDto>.Success(_mapper.Map<WorkSpaceDto>(workspace));
        }
    }
}
