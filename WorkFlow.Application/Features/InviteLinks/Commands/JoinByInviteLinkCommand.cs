using FluentValidation;
using MediatR;
using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.InviteLinks.Commands
{
    public record JoinByInviteLinkCommand(
        string Token,
        string? Slug = null
    ) : IRequest<Result>;


    public class JoinByInviteLinkCommandValidator
        : AbstractValidator<JoinByInviteLinkCommand>
    {
        public JoinByInviteLinkCommandValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty()
                .WithMessage("Token không được để trống.");

            RuleFor(x => x.Slug)
                .MaximumLength(50)
                .When(x => !string.IsNullOrWhiteSpace(x.Slug));
        }
    }

    public class JoinByInviteLinkCommandHandler
    : IRequestHandler<JoinByInviteLinkCommand, Result>
    {
        private readonly IRepository<InviteLink, Guid> _inviteLinkRepository;
        private readonly IRepository<WorkspaceMember, Guid> _workspaceMemberRepository;
        private readonly IRepository<BoardMember, Guid> _boardMemberRepository;
        private readonly IRepository<Board, Guid> _boardRepository;

        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public JoinByInviteLinkCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser)
        {
            _inviteLinkRepository = unitOfWork.GetRepository<InviteLink, Guid>();
            _workspaceMemberRepository = unitOfWork.GetRepository<WorkspaceMember, Guid>();
            _boardMemberRepository = unitOfWork.GetRepository<BoardMember, Guid>();
            _boardRepository = unitOfWork.GetRepository<Board, Guid>();

            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            JoinByInviteLinkCommand request,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserIdOrFail();

            var inviteLink = await LoadInviteLinkOrFail(request);

            var expireResult = await EnsureInviteLinkActive(inviteLink);
            if (!expireResult.IsSuccess)
                return expireResult;

            EnsureInvitedUserMatch(inviteLink, userId);

            return inviteLink.Type switch
            {
                InviteLinkType.WorkSpace => await JoinWorkspace(inviteLink, userId),
                InviteLinkType.Board => await JoinBoard(inviteLink, userId),
                _ => Result.Failure("InviteLinkType không hợp lệ.")
            };
        }

        private Guid GetCurrentUserIdOrFail()
        {
            if (_currentUser.UserId == null)
                throw new ForbiddenAccessException("Bạn cần đăng nhập để tham gia.");

            return _currentUser.UserId.Value;
        }

        private async Task<InviteLink> LoadInviteLinkOrFail(JoinByInviteLinkCommand request)
        {
            var inviteLink = await _inviteLinkRepository.FirstOrDefaultAsync(x =>
                x.Token == request.Token &&
                (request.Slug == null || x.Slug == request.Slug)
            );

            if (inviteLink == null)
                throw new DomainException("Invite Link không hợp lệ.");

            return inviteLink;
        }

        private async Task<Result> EnsureInviteLinkActive(InviteLink inviteLink)
        {
            inviteLink.CheckAndExpireByTime();

            if (inviteLink.Status == InviteLinkStatus.Expired)
            {
                await _inviteLinkRepository.UpdateAsync(inviteLink);
                await _unitOfWork.SaveChangesAsync();
                return Result.Failure("Invite Link đã hết hạn.");
            }

            if (inviteLink.Status != InviteLinkStatus.Active)
                return Result.Failure("Invite Link không còn hiệu lực.");

            return Result.Success();
        }

        private static void EnsureInvitedUserMatch(InviteLink inviteLink, Guid userId)
        {
            if (inviteLink.InvitedUserId.HasValue &&
                inviteLink.InvitedUserId.Value != userId)
                throw new DomainException("Invite Link này không dành cho bạn.");
        }

        private async Task<Result> JoinWorkspace(InviteLink inviteLink, Guid userId)
        {
            var exists = await _workspaceMemberRepository.AnyAsync(
                x => x.WorkSpaceId == inviteLink.TargetId && x.UserId == userId
            );

            if (exists)
                return Result.Failure("Bạn đã là thành viên của Workspace.");

            var member = WorkspaceMember.Create(
                inviteLink.TargetId,
                userId,
                WorkSpaceRole.Member
            );

            await _workspaceMemberRepository.AddAsync(member);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Tham gia Workspace thành công.");
        }

        private async Task<Result> JoinBoard(InviteLink inviteLink, Guid userId)
        {
            var board = await _boardRepository.GetByIdAsync(inviteLink.TargetId);
            if (board == null)
                return Result.Failure("Board không tồn tại.");

            var inWorkspace = await _workspaceMemberRepository.AnyAsync(
                x => x.WorkSpaceId == board.WorkSpaceId && x.UserId == userId
            );

            if (!inWorkspace)
                return Result.Failure("Bạn phải là thành viên Workspace trước khi tham gia Board.");

            var inBoard = await _boardMemberRepository.AnyAsync(
                x => x.BoardId == board.Id && x.UserId == userId
            );

            if (inBoard)
                return Result.Failure("Bạn đã là thành viên của Board.");

            var member = BoardMember.Create(
                board.Id,
                userId,
                BoardRole.Viewer
            );

            await _boardMemberRepository.AddAsync(member);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Tham gia Board thành công.");
        }
    }

}
