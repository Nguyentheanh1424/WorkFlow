using AutoMapper;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Application.Features.Comments.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Comments.Queries
{
    public record GetCommentsQuery(Guid CardId)
        : IRequest<Result<List<CommentWithUserDto>>>;

    public class GetCommentsQueryValidator
        : AbstractValidator<GetCommentsQuery>
    {
        public GetCommentsQueryValidator()
        {
            RuleFor(x => x.CardId)
                .NotEmpty()
                .WithMessage("CardId không được để trống.");
        }
    }

    public class GetCommentsQueryHandler
        : IRequestHandler<GetCommentsQuery, Result<List<CommentWithUserDto>>>
    {
        private readonly IRepository<Comment, Guid> _commentRepository;
        private readonly IRepository<Card, Guid> _cardRepository;
        private readonly IRepository<User, Guid> _userRepository;

        private readonly IPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;

        public GetCommentsQueryHandler(
            IUnitOfWork unitOfWork,
            IPermissionService permission,
            ICurrentUserService currentUser,
            IMapper mapper)
        {
            _commentRepository = unitOfWork.GetRepository<Comment, Guid>();
            _cardRepository = unitOfWork.GetRepository<Card, Guid>();
            _userRepository = unitOfWork.GetRepository<User, Guid>();

            _permission = permission;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<Result<List<CommentWithUserDto>>> Handle(
            GetCommentsQuery request,
            CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<List<CommentWithUserDto>>.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var card = await _cardRepository.GetByIdAsync(request.CardId)
                ?? throw new NotFoundException("Card không tồn tại.");

            await _permission.Card.EnsureCanViewAsync(card.Id, userId);

            var comments = await _commentRepository.FindAsync(
                c => c.CardId == card.Id
            );

            var ordered = comments
                .OrderBy(c => c.CreatedAt)
                .ToList();

            var userIds = ordered
                .Select(c => c.UserId)
                .Distinct()
                .ToList();

            var users = await _userRepository.FindAsync(
                u => userIds.Contains(u.Id)
            );

            var userDict = users.ToDictionary(u => u.Id);

            var dtos = ordered.Select(c => new CommentWithUserDto
            {
                Id = c.Id,
                CardId = c.CardId,
                UserId = c.UserId,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,

                UserName = userDict.TryGetValue(c.UserId, out var u)
                    ? u.Name
                    : "Unknown",
                UserAvatar = userDict.TryGetValue(c.UserId, out var u2)
                    ? u2.AvatarUrl
                    : string.Empty
            }).ToList();

            return Result<List<CommentWithUserDto>>.Success(dtos);
        }
    }
}
