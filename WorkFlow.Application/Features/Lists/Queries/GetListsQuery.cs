using AutoMapper;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Application.Features.Lists.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Lists.Queries
{
    public record GetListsQuery(Guid BoardId)
        : IRequest<Result<List<ListDto>>>;

    public class GetListsQueryValidator : AbstractValidator<GetListsQuery>
    {
        public GetListsQueryValidator()
        {
            RuleFor(x => x.BoardId)
                .NotEmpty().WithMessage("BoardId không được để trống.");
        }
    }

    public class GetListsQueryHandler
        : IRequestHandler<GetListsQuery, Result<List<ListDto>>>
    {
        private readonly IRepository<List, Guid> _listRepository;
        private readonly IRepository<Board, Guid> _boardRepository;
        private readonly IBoardPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;

        public GetListsQueryHandler(
            IBoardPermissionService permission,
            ICurrentUserService currentUser,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _listRepository = unitOfWork.GetRepository<List, Guid>();
            _boardRepository = unitOfWork.GetRepository<Board, Guid>();
            _permission = permission;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<Result<List<ListDto>>> Handle(GetListsQuery request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<List<ListDto>>.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var board = await _boardRepository.GetByIdAsync(request.BoardId);
            if (board == null)
                return Result<List<ListDto>>.Failure("Board không tồn tại.");

            await _permission.EnsureViewerAsync(board.Id, userId);

            var lists = await _listRepository.FindAsync(l => l.BoardId == board.Id);

            var ordered = lists.OrderBy(l => l.Position).ToList();

            var dto = _mapper.Map<List<ListDto>>(ordered);

            return Result<List<ListDto>>.Success(dto);
        }
    }
}
