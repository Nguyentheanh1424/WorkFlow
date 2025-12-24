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
        : IRequest<Result<List<CommentDto>>>;

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
        : IRequestHandler<GetCommentsQuery, Result<List<CommentDto>>>
    {
        private readonly IRepository<Comment, Guid> _commentRepository;
        private readonly IRepository<Card, Guid> _cardRepository;

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

            _permission = permission;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<Result<List<CommentDto>>> Handle(
            GetCommentsQuery request,
            CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<List<CommentDto>>.Failure("Không xác định được người dùng.");

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

            var dtos = _mapper.Map<List<CommentDto>>(ordered);

            return Result<List<CommentDto>>.Success(dtos);
        }
    }
}
