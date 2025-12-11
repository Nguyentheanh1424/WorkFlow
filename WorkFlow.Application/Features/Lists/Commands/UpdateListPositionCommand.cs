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
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Application.Features.Lists.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Lists.Commands
{
    public record UpdateListPositionCommand(Guid ListId, int NewPosition)
       : IRequest<Result<ListDto>>;

    public class UpdateListPositionCommandValidator : AbstractValidator<UpdateListPositionCommand>
    {
        public UpdateListPositionCommandValidator()
        {
            RuleFor(x => x.ListId)
                .NotEmpty().WithMessage("ListId không được để trống.");

            RuleFor(x => x.NewPosition)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Vị trí phải >= 0.");
        }
    }

    public class UpdateListPositionCommandHandler
        : IRequestHandler<UpdateListPositionCommand, Result<ListDto>>
    {
        private readonly IRepository<List, Guid> _listRepository;
        private readonly IRepository<Board, Guid> _boardRepository;
        private readonly IBoardPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IRealtimeService _realtime;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateListPositionCommandHandler(
            IBoardPermissionService permission,
            ICurrentUserService currentUser,
            IRealtimeService realtime,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _listRepository = unitOfWork.GetRepository<List, Guid>();
            _boardRepository = unitOfWork.GetRepository<Board, Guid>();
            _permission = permission;
            _currentUser = currentUser;
            _realtime = realtime;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<ListDto>> Handle(UpdateListPositionCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<ListDto>.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var list = await _listRepository.GetByIdAsync(request.ListId);
            if (list == null)
                return Result<ListDto>.Failure("List không tồn tại.");

            var board = await _boardRepository.GetByIdAsync(list.BoardId);
            if (board == null)
                return Result<ListDto>.Failure("Board không tồn tại.");

            await _permission.EnsureEditorAsync(board.Id, userId);

            var lists = await _listRepository.FindAsync(l => l.BoardId == board.Id);

            var ordered = lists.OrderBy(l => l.Position).ToList();

            ordered.RemoveAll(l => l.Id == list.Id);

            var newPosition = Math.Min(request.NewPosition, ordered.Count);
            ordered.Insert(newPosition, list);

            for (int i = 0; i < ordered.Count; i++)
            {
                ordered[i].MoveTo(i);
            }

            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<ListDto>(list);

            await _realtime.SendToBoardAsync(board.Id, ListEvents.Updated, dto);

            return Result<ListDto>.Success(dto);
        }
    }
}
