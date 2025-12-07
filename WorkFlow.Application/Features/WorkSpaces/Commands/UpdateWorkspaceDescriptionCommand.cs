using FluentValidation;
using MediatR;
using AutoMapper;
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Application.Features.WorkSpaces.Dtos;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Common;

namespace WorkFlow.Application.Features.WorkSpaces.Commands
{
    public record UpdateWorkspaceDescriptionCommand(Guid Id, string? Description) : IRequest<Result<WorkSpaceDto>>;

    public class UpdateWorkspaceDescriptionCommandValidator : AbstractValidator<UpdateWorkspaceDescriptionCommand>
    {
        public UpdateWorkspaceDescriptionCommandValidator()
        {
            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description không được vượt quá 1000 ký tự.");
        }
    }

    public class UpdateWorkspaceDescriptionCommandHandler : IRequestHandler<UpdateWorkspaceDescriptionCommand, Result<WorkSpaceDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<WorkSpace, Guid> _repository;
        private readonly IMapper _mapper;

        public UpdateWorkspaceDescriptionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _repository = _unitOfWork.GetRepository<WorkSpace, Guid>();
            _mapper = mapper;
        }

        public async Task<Result<WorkSpaceDto>> Handle(UpdateWorkspaceDescriptionCommand request, CancellationToken cancellationToken)
        {
            var workspace = await _repository.GetByIdAsync(request.Id);
            if (workspace == null)
                return Result<WorkSpaceDto>.Failure("Workspace không tồn tại.");

            workspace.UpdateDescription(request.Description);

            await _unitOfWork.SaveChangesAsync();

            return Result<WorkSpaceDto>.Success(_mapper.Map<WorkSpaceDto>(workspace));
        }
    }
}
