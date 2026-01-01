using AutoMapper;
using FluentValidation;
using MediatR;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Application.Features.Cards.Dtos;
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
        private readonly IRepository<Card, Guid> _cardRepository;
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
            _cardRepository = unitOfWork.GetRepository<Card, Guid>();
            _permission = permission;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<Result<List<ListDto>>> Handle(
            GetListsQuery request,
            CancellationToken cancellationToken)
        {
            if (_currentUser.UserId is null)
                return Result<List<ListDto>>.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var board = await _boardRepository.GetByIdAsync(request.BoardId);
            if (board is null)
                return Result<List<ListDto>>.Failure("Board không tồn tại.");

            await _permission.EnsureViewerAsync(board.Id, userId);

            var lists = (await _listRepository.FindAsync(
                    l => l.BoardId == board.Id && !l.IsArchived))
                .OrderBy(l => l.Position)
                .ToList();

            if (!lists.Any())
                return Result<List<ListDto>>.Success(new List<ListDto>());

            var listIds = lists.Select(l => l.Id).ToList();

            var cards = (await _cardRepository.FindAsync(
                    c => listIds.Contains(c.ListId)))
                .OrderBy(c => c.ListId)
                .ThenBy(c => c.Position)
                .ToList();

            var listDtos = _mapper.Map<List<ListDto>>(lists);
            var cardDtos = _mapper.Map<List<CardDto>>(cards);

            var cardLookup = cardDtos.GroupBy(c => c.ListId)
                                     .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var listDto in listDtos)
            {
                listDto.Cards = cardLookup.TryGetValue(listDto.Id, out var listCards)
                    ? listCards
                    : new List<CardDto>();
            }

            return Result<List<ListDto>>.Success(listDtos);
        }
    }
}
