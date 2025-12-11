using FluentValidation;
using MediatR;
using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Application.Features.CardAssignees.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.CardAssignees.Queries
{
    public record GetCardAssigneesQuery(Guid CardId)
        : IRequest<Result<List<CardAssigneeDto>>>;

    public class GetCardAssigneesQueryValidator : AbstractValidator<GetCardAssigneesQuery>
    {
        public GetCardAssigneesQueryValidator()
        {
            RuleFor(x => x.CardId).NotEmpty();
        }
    }

    public class GetCardAssigneesQueryHandler
    : IRequestHandler<GetCardAssigneesQuery, Result<List<CardAssigneeDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBoardPermissionService _permission;
        private readonly ICurrentUserService _currentUser;

        public GetCardAssigneesQueryHandler(
            IUnitOfWork unitOfWork,
            IBoardPermissionService permission,
            ICurrentUserService currentUser
            )
        {
            _unitOfWork = unitOfWork;
            _permission = permission;
            _currentUser = currentUser;
        }

        public async Task<Result<List<CardAssigneeDto>>> Handle(GetCardAssigneesQuery request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<List<CardAssigneeDto>>.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var cardRepo = _unitOfWork.GetRepository<Card, Guid>();
            var listRepo = _unitOfWork.GetRepository<List, Guid>();
            var boardRepo = _unitOfWork.GetRepository<Board, Guid>();
            var assigneeRepo = _unitOfWork.GetRepository<CardAssignee, Guid>();
            var userRepo = _unitOfWork.GetRepository<User, Guid>();

            var card = await cardRepo.GetByIdAsync(request.CardId);
            if (card == null)
                return Result<List<CardAssigneeDto>>.Failure("Card không tồn tại.");

            var list = await listRepo.GetByIdAsync(card.ListId)
                ?? throw new NotFoundException("List không tồn tại.");

            var board = await boardRepo.GetByIdAsync(list.BoardId)
                ?? throw new NotFoundException("Board không tồn tại.");

            await _permission.EnsureViewerAsync(board.Id, userId);

            var assignments = await assigneeRepo.FindAsync(x => x.CardId == card.Id);

            var userIds = assignments.Select(x => x.UserId).Distinct().ToList();
            var users = await userRepo.FindAsync(x => userIds.Contains(x.Id));
            var lookup = users.ToDictionary(x => x.Id);

            var dtos = assignments.Select(a =>
            {
                lookup.TryGetValue(a.UserId, out var user);

                return new CardAssigneeDto
                {
                    UserId = a.UserId,
                    UserName = user?.Name ?? "(Unknown)",
                    CreatedAt = a.CreatedAt
                };
            }).ToList();

            return Result<List<CardAssigneeDto>>.Success(dtos);
        }
    }
}
