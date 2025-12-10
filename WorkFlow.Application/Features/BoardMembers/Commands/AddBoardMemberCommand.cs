using FluentValidation;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using WorkFlow.Application.Common.Constants.EventNames;
using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Application.Features.BoardMembers.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.BoardMembers.Commands
{
    public record AddBoardMemberCommand(Guid BoardId, Guid UserId, BoardRole Role)
        : IRequest<Result<BoardMemberDto>>;

    public class AddBoardMemberCommandValidator : AbstractValidator<AddBoardMemberCommand>
    {
        public AddBoardMemberCommandValidator()
        {
            RuleFor(x => x.BoardId).NotEmpty();
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.Role).IsInEnum();
        }
    }

    public class AddBoardMemberCommandHandler
    : IRequestHandler<AddBoardMemberCommand, Result<BoardMemberDto>>
    {
        private readonly IRepository<BoardMember, Guid> _boardMemberRepository;
        private readonly IRepository<Board, Guid> _boardRepository;
        private readonly IRepository<User, Guid> _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IRealtimeService _realtime;
        private readonly IPermissionService _permission;

        public AddBoardMemberCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IRealtimeService realtime,
            IPermissionService permission)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _realtime = realtime;
            _permission = permission;

            _boardMemberRepository = unitOfWork.GetRepository<BoardMember, Guid>();
            _boardRepository = unitOfWork.GetRepository<Board, Guid>();
            _userRepository = unitOfWork.GetRepository<User, Guid>();
        }

        public async Task<Result<BoardMemberDto>> Handle(AddBoardMemberCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                throw new ForbiddenAccessException("Không xác định được người dùng.");

            var currentUserId = _currentUser.UserId.Value;

            var board = await _boardRepository.GetByIdAsync(request.BoardId)
                ?? throw new NotFoundException("Board không tồn tại.");

            await _permission.Board.EnsureCanAssignRoleAsync(
                board.Id,
                currentUserId,
                request.Role
            );

            await _permission.Workspace.EnsureMemberAsync(board.WorkSpaceId, request.UserId);

            var user = await _userRepository.GetByIdAsync(request.UserId)
                ?? throw new NotFoundException("User không tồn tại.");

            var exists = await _boardMemberRepository.AnyAsync(
                x => x.BoardId == board.Id && x.UserId == request.UserId
            );

            if (exists)
                return Result<BoardMemberDto>.Failure("User đã thuộc board.");

            var member = BoardMember.Create(board.Id, request.UserId, request.Role);
            await _boardMemberRepository.AddAsync(member);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = new BoardMemberDto
            {
                UserId = member.UserId,
                Name = user.Name,
                Role = member.Role,
                JoinedAt = member.JoinedAt
            };

            await _realtime.SendToUserAsync(
                request.UserId,
                BoardEvents.MemberAdded,
                dto
            );

            return Result<BoardMemberDto>.Success(dto);
        }
    }

}
