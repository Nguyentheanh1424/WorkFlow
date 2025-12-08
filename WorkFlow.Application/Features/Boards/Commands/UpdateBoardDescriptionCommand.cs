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
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Application.Features.Boards.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Boards.Commands
{
    public record UpdateBoardDescriptionCommand(Guid BoardId, string Description)
        : IRequest<Result>;

    public class UpdateBoardDescriptionCommandValidator
        : AbstractValidator<UpdateBoardDescriptionCommand>
    {
        public UpdateBoardDescriptionCommandValidator()
        {
            RuleFor(x => x.BoardId)
                .NotEmpty().WithMessage("BoardId không được để trống.");

            RuleFor(x => x.Description)
                .MaximumLength(2000)
                .WithMessage("Mô tả không được vượt quá 2000 ký tự.");
        }
    }
    public class UpdateBoardDescriptionCommandHandler
        : IRequestHandler<UpdateBoardDescriptionCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Board, Guid> _boardRepository;
        private readonly IBoardPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IRealtimeService _realtimeService;
        private readonly IMapper _mapper;

        public UpdateBoardDescriptionCommandHandler(
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
            _realtimeService = realtime;
            _mapper = mapper;
        }

        public async Task<Result> Handle(UpdateBoardDescriptionCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var board = await _boardRepository.GetByIdAsync(request.BoardId)
                ?? throw new NotFoundException("Board không tồn tại.");

            await _permission.EnsureEditorAsync(board.Id, userId);

            board.SetDescription(request.Description);

            await _boardRepository.UpdateAsync(board);
            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<BoardDto>(board);

            await _realtimeService.SendToBoardAsync(board.Id, BoardEvents.Updated, dto);
            await _realtimeService.SendToWorkspaceAsync(board.WorkspaceId, BoardEvents.Updated, dto);

            return Result.Success();
        }
    }
}
