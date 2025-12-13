using AutoMapper;
using FluentValidation;
using MediatR;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Application.Features.InviteLinks.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.InviteLinks.Commands
{
    public record CreateInviteLinkCommand(
        InviteLinkType Type,
        Guid TargetId,
        Guid? InvitedUserId,
        string? Slug,
        DateTime? ExpiredAt
    ) : IRequest<Result<InviteLinkDto>>;

    public class CreateInviteLinkCommandValidator
        : AbstractValidator<CreateInviteLinkCommand>
    {
        public CreateInviteLinkCommandValidator()
        {
            RuleFor(x => x.TargetId)
                .NotEmpty()
                .WithMessage("TargetId không được để trống.");

            RuleFor(x => x.Slug)
                .MaximumLength(50)
                .When(x => !string.IsNullOrWhiteSpace(x.Slug))
                .WithMessage("Slug không được vượt quá 50 ký tự.");

            RuleFor(x => x.ExpiredAt)
                .Must(x => x > DateTime.UtcNow)
                .When(x => x.ExpiredAt.HasValue)
                .WithMessage("ExpiredAt phải lớn hơn thời điểm hiện tại.");
        }
    }

    public class CreateInviteLinkCommandHandler
        : IRequestHandler<CreateInviteLinkCommand, Result<InviteLinkDto>>
    {
        private readonly IRepository<InviteLink, Guid> _inviteLinkRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IPermissionService _permission;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateInviteLinkCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IPermissionService permission,
            IMapper mapper)
        {
            _inviteLinkRepository = unitOfWork.GetRepository<InviteLink, Guid>();
            _currentUser = currentUser;
            _permission = permission;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<InviteLinkDto>> Handle(
            CreateInviteLinkCommand request,
            CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<InviteLinkDto>.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            if (request.Type == InviteLinkType.WorkSpace)
            {
                await _permission.Workspace.EnsureAdminAsync(request.TargetId, userId);
            }
            else if (request.Type == InviteLinkType.Board)
            {
                await _permission.Board.EnsureOwnerAsync(request.TargetId, userId);
            }
            else
            {
                return Result<InviteLinkDto>.Failure("InviteLinkType không hợp lệ.");
            }

            var inviteLink = InviteLink.Create(
                type: request.Type,
                targetId: request.TargetId,
                invitedUserId: request.InvitedUserId,
                slug: request.Slug,
                expiredAt: request.ExpiredAt
            );

            await _inviteLinkRepository.AddAsync(inviteLink);
            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<InviteLinkDto>(inviteLink);

            return Result<InviteLinkDto>.Success(dto);
        }
    }
}
