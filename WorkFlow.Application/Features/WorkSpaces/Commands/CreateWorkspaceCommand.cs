using AutoMapper;
using FluentValidation;
using MediatR;
using WorkFlow.Application.Common.Constants.EventNames;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Application.Features.WorkSpaces.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.WorkSpaces.Commands
{
    public record CreateWorkspaceCommand(CreateWorkspaceDto WorkSpace) : IRequest<Result<WorkSpaceDto>>;

    public class CreateWorkspaceCommandValidator : AbstractValidator<CreateWorkspaceCommand>
    {
        public CreateWorkspaceCommandValidator()
        {
            RuleFor(x => x.WorkSpace.Type)
                .IsInEnum().WithMessage("Type không hợp lệ.");

            RuleFor(x => x.WorkSpace.Name)
                .NotEmpty().WithMessage("Tên không được để trống.")
                .MaximumLength(255).WithMessage("Tên WorkSpace không được vượt quá 255 ký tự.");
        }
    }

    public class CreateWorkspaceCommandHandler : IRequestHandler<CreateWorkspaceCommand, Result<WorkSpaceDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<WorkSpace, Guid> _workspaceRepository;
        private readonly IRepository<WorkspaceMember, Guid> _workspaceMemberRepository;
        private readonly IRealtimeService _realtimeService;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;

        public CreateWorkspaceCommandHandler(
            IUnitOfWork unitOfWork,
            IRealtimeService realtimeService,
            ICurrentUserService currentUser,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _realtimeService = realtimeService;
            _currentUser = currentUser;
            _mapper = mapper;

            _workspaceRepository = unitOfWork.GetRepository<WorkSpace, Guid>();
            _workspaceMemberRepository = unitOfWork.GetRepository<WorkspaceMember, Guid>();
        }

        public async Task<Result<WorkSpaceDto>> Handle(CreateWorkspaceCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<WorkSpaceDto>.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var workSpace = WorkSpace.Create(
                request.WorkSpace.Name,
                request.WorkSpace.Type,
                request.WorkSpace.Background,
                request.WorkSpace.Description
            );

            await _workspaceRepository.AddAsync(workSpace);

            var ownerMember = WorkspaceMember.Create(workSpace.Id, userId, WorkSpaceRole.Owner);
            await _workspaceMemberRepository.AddAsync(ownerMember);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = _mapper.Map<WorkSpaceDto>(workSpace);

            await _realtimeService.SendToUserAsync(
                userId,
                WorkspaceEvents.Create,
                dto
            );

            return Result<WorkSpaceDto>.Success(dto);
        }
    }
}
