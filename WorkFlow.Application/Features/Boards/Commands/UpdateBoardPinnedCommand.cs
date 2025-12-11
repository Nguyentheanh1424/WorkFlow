using AutoMapper;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow.Application.Common.Constants.EventNames;
using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Application.Features.Boards.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Boards.Commands
{
    public record UpdateBoardPinnedCommand(Guid BoardId, bool Pinned)
        : IRequest<Result>;

    public class UpdateBoardPinnedCommandValidator
        : AbstractValidator<UpdateBoardPinnedCommand>
    {
        public UpdateBoardPinnedCommandValidator()
        {
            RuleFor(x => x.BoardId)
                .NotEmpty().WithMessage("BoardId không được để trống.");
        }
    }

    public class UpdateBoardPinnedCommandHandler
        : IRequestHandler<UpdateBoardPinnedCommand, Result>
    {
        private readonly IRepository<Board, Guid> _boardRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBoardPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IRealtimeService _realtime;
        private readonly IMapper _mapper;

        public UpdateBoardPinnedCommandHandler(
            IUnitOfWork unitOfWork,
            IBoardPermissionService permission,
            ICurrentUserService currentUser,
            IRealtimeService realtime,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _boardRepository = unitOfWork.GetRepository<Board, Guid>();
            _permission = permission;
            _currentUser = currentUser;
            _realtime = realtime;
            _mapper = mapper;
        }

        public async Task<Result> Handle(UpdateBoardPinnedCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var board = await _boardRepository.GetByIdAsync(request.BoardId)
                ?? throw new NotFoundException("Board không tồn tại.");

            await _permission.EnsureEditorAsync(board.Id, userId);

            if (request.Pinned)
                board.Pin();
            else
                board.Unpin();

            await _boardRepository.UpdateAsync(board);
            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<BoardDto>(board);

            await _realtime.SendToBoardAsync(board.Id, BoardEvents.Updated, dto);
            await _realtime.SendToWorkspaceAsync(board.WorkSpaceId, BoardEvents.Updated, dto);

            return Result.Success();
        }
    }
}
