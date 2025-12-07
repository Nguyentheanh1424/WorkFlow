using AutoMapper;
using FluentValidation;
using MediatR;
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Application.Features.WorkSpaces.Dtos;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.WorkSpaces.Commands
{
    public record UpdateWorkspaceNameCommand(Guid Id, string Name) : IRequest<Result<WorkSpaceDto>>;

    public class UpdateWorkspaceNameCommandValidator : AbstractValidator<UpdateWorkspaceNameCommand>
    {
        public UpdateWorkspaceNameCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên không được để trống.")
                .MaximumLength(255).WithMessage("Tên không được vượt quá 255 ký tự.");
        }
    }

    public class UpdateWorkspaceNameCommandHandler : IRequestHandler<UpdateWorkspaceNameCommand, Result<WorkSpaceDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<WorkSpace, Guid> _repository;
        private readonly IMapper _mapper;

        public UpdateWorkspaceNameCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _repository = _unitOfWork.GetRepository<WorkSpace, Guid>();
            _mapper = mapper;
        }

        public async Task<Result<WorkSpaceDto>> Handle(UpdateWorkspaceNameCommand request, CancellationToken cancellationToken)
        {
            var workspace = await _repository.GetByIdAsync(request.Id);
            if (workspace == null)
                return Result<WorkSpaceDto>.Failure("Workspace không tồn tại.");

            workspace.UpdateName(request.Name);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<WorkSpaceDto>.Success(_mapper.Map<WorkSpaceDto>(workspace));
        }
    }
}
