using AutoMapper;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow.Application.Common.Constants.EventNames;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Application.Features.Boards.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Boards.Commands
{
    public record UpdateBoardBackgroundCommand(Guid BoardId, string Background)
        : IRequest<Result>;

    public class UpdateBoardBackgroundCommandValidator
        : AbstractValidator<UpdateBoardBackgroundCommand>
    {
        public UpdateBoardBackgroundCommandValidator()
        {
            RuleFor(x => x.BoardId)
                .NotEmpty().WithMessage("BoardId không được để trống.");

            RuleFor(x => x.Background)
                .MaximumLength(2000)
                .WithMessage("Background không hợp lệ.")
                .NotNull().WithMessage("Background không được null.");
        }
    }

    public class UpdateBoardBackgroundCommandHandler
        : IRequestHandler<UpdateBoardBackgroundCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Board, Guid> _boardRepository;
        private readonly IBoardPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IRealtimeService _realtime;
        private readonly IMapper _mapper;

        public UpdateBoardBackgroundCommandHandler(
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

        public async Task<Result> Handle(UpdateBoardBackgroundCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var board = await _boardRepository.GetByIdAsync(request.BoardId);
            if (board == null)
                return Result.Failure("Board không tồn tại.");

            await _permission.EnsureEditorAsync(board.Id, userId);

            board.SetBackground(request.Background);

            await _boardRepository.UpdateAsync(board);
            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<BoardDto>(board);

            await _realtime.SendToBoardAsync(board.Id, BoardEvents.Updated, dto);
            await _realtime.SendToWorkspaceAsync(board.WorkSpaceId, BoardEvents.Updated, dto);

            return Result.Success();
        }
    }
}
