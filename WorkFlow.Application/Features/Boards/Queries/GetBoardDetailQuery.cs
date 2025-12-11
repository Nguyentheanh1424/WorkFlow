using AutoMapper;
using MediatR;
using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Application.Features.Boards.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Boards.Queries
{
    public record GetBoardDetailQuery(Guid BoardId)
    : IRequest<Result<BoardDto>>;

    public class GetBoardDetailQueryHandler
    : IRequestHandler<GetBoardDetailQuery, Result<BoardDto>>
    {
        private readonly IRepository<Board, Guid> _boardRepository;
        private readonly IBoardPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;

        public GetBoardDetailQueryHandler(
            IUnitOfWork unitOfWork,
            IBoardPermissionService permission,
            ICurrentUserService currentUser,
            IMapper mapper)
        {
            _boardRepository = unitOfWork.GetRepository<Board, Guid>();
            _permission = permission;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<Result<BoardDto>> Handle(GetBoardDetailQuery request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<BoardDto>.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var board = await _boardRepository.GetByIdAsync(request.BoardId)
                ?? throw new NotFoundException("Board không tồn tại.");

            await _permission.EnsureViewerAsync(board.Id, userId);

            var dto = _mapper.Map<BoardDto>(board);
            return Result<BoardDto>.Success(dto);
        }
    }

}
