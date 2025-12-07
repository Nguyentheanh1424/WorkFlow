using FluentValidation;
using MediatR;
using AutoMapper;
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Application.Features.WorkSpaces.Dtos;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.WorkSpaces.Commands
{
    public record UpdateWorkspaceTypeCommand(Guid Id, WorkSpaceType Type) : IRequest<Result<WorkSpaceDto>>;

    public class UpdateWorkspaceTypeCommandValidator : AbstractValidator<UpdateWorkspaceTypeCommand>
    {
        public UpdateWorkspaceTypeCommandValidator()
        {
            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Type không hợp lệ.");
        }
    }

    public class UpdateWorkspaceTypeCommandHandler : IRequestHandler<UpdateWorkspaceTypeCommand, Result<WorkSpaceDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<WorkSpace, Guid> _repository;
        private readonly IMapper _mapper;

        public UpdateWorkspaceTypeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _repository = _unitOfWork.GetRepository<WorkSpace, Guid>();
            _mapper = mapper;
        }

        public async Task<Result<WorkSpaceDto>> Handle(UpdateWorkspaceTypeCommand request, CancellationToken cancellationToken)
        {
            var workspace = await _repository.GetByIdAsync(request.Id);
            if (workspace == null)
                return Result<WorkSpaceDto>.Failure("Workspace không tồn tại.");

            workspace.UpdateType(request.Type);

            await _unitOfWork.SaveChangesAsync();

            return Result<WorkSpaceDto>.Success(_mapper.Map<WorkSpaceDto>(workspace));
        }
    }
}
