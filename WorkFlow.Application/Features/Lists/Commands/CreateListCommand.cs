using AutoMapper;
using FluentValidation;
using MediatR;
using WorkFlow.Application.Common.Constants.EventNames;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Application.Features.Lists.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Lists.Commands
{
    public record CreateListCommand(Guid BoardId, string Title)
        : IRequest<Result<ListDto>>;

    public class CreateListCommandValidator : AbstractValidator<CreateListCommand>
    {
        public CreateListCommandValidator()
        {
            RuleFor(x => x.BoardId)
                .NotEmpty().WithMessage("BoardId không được để trống.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title không được để trống.")
                .MaximumLength(200).WithMessage("Title không được vượt quá 200 ký tự.");
        }
    }

    public class CreateListCommandHandler
        : IRequestHandler<CreateListCommand, Result<ListDto>>
    {
        private readonly IRepository<List, Guid> _listRepository;
        private readonly IRepository<Board, Guid> _boardRepository;
        private readonly IBoardPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IRealtimeService _realtime;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateListCommandHandler(
            IUnitOfWork unitOfWork,
            IBoardPermissionService permission,
            ICurrentUserService currentUser,
            IRealtimeService realtime,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _listRepository = unitOfWork.GetRepository<List, Guid>();
            _boardRepository = unitOfWork.GetRepository<Board, Guid>();
            _permission = permission;
            _currentUser = currentUser;
            _realtime = realtime;
            _mapper = mapper;
        }

        public async Task<Result<ListDto>> Handle(CreateListCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<ListDto>.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var board = await _boardRepository.GetByIdAsync(request.BoardId);
            if (board == null)
                return Result<ListDto>.Failure("Board không tồn tại.");

            await _permission.EnsureEditorAsync(board.Id, userId);

            var lists = await _listRepository.FindAsync(l => l.BoardId == board.Id);
            var nextPosition = lists.Count == 0 ? 0 : lists.Max(l => l.Position) + 1;

            var newList = List.Create(board.Id, request.Title, nextPosition);

            await _listRepository.AddAsync(newList);
            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<ListDto>(newList);

            await _realtime.SendToBoardAsync(board.Id, ListEvents.Created, dto);

            return Result<ListDto>.Success(dto);
        }
    }
}
