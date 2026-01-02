using AutoMapper;
using FluentValidation;
using MediatR;
using WorkFlow.Application.Common.Constants.EventNames;
using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Application.Features.Comments.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Comments.Commands
{
    public class CreateCommentCommand : IRequest<Result<CommentWithUserDto>>
    {
        public Guid CardId { get; private set; }
        public string Content { get; set; } = null!;

        public void SetCardId(Guid cardId)
        {
            CardId = cardId;
        }
    }

    public class CreateCommentCommandValidator
        : AbstractValidator<CreateCommentCommand>
    {
        public CreateCommentCommandValidator()
        {
            RuleFor(x => x.CardId)
                .NotEmpty()
                .WithMessage("CardId không được để trống.");

            RuleFor(x => x.Content)
                .NotEmpty()
                .WithMessage("Nội dung comment không được để trống.")
                .MaximumLength(2000)
                .WithMessage("Nội dung comment tối đa 2000 ký tự.");
        }
    }

    public class CreateCommentCommandHandler
        : IRequestHandler<CreateCommentCommand, Result<CommentWithUserDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Card, Guid> _cardRepository;
        private readonly IRepository<Comment, Guid> _commentRepository;
        private readonly IRepository<List, Guid> _listRepository;
        private readonly IRepository<User, Guid> _userRepository;

        private readonly IPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IRealtimeService _realtimeService;
        private readonly IMapper _mapper;

        public CreateCommentCommandHandler(
            IUnitOfWork unitOfWork,
            IPermissionService permission,
            ICurrentUserService currentUser,
            IRealtimeService realtime,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _cardRepository = unitOfWork.GetRepository<Card, Guid>();
            _commentRepository = unitOfWork.GetRepository<Comment, Guid>();
            _listRepository = unitOfWork.GetRepository<List, Guid>();
            _userRepository = unitOfWork.GetRepository<User, Guid>();

            _permission = permission;
            _currentUser = currentUser;
            _realtimeService = realtime;
            _mapper = mapper;
        }


        public async Task<Result<CommentWithUserDto>> Handle(
            CreateCommentCommand request,
            CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<CommentWithUserDto>.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var card = await _cardRepository.GetByIdAsync(request.CardId)
                ?? throw new NotFoundException("Card không tồn tại.");

            var list = await _listRepository.GetByIdAsync(card.ListId)
                ?? throw new NotFoundException("Danh sách không tồn tại.");

            await _permission.Card.EnsureCanCommentAsync(card.Id, userId);

            var comment = Comment.Create(
                cardId: card.Id,
                userId: userId,
                content: request.Content.Trim()
            );

            await _commentRepository.AddAsync(comment);
            await _unitOfWork.SaveChangesAsync();

            var user = await _userRepository.GetByIdAsync(userId);

            var dto = new CommentWithUserDto
            {
                Id = comment.Id,
                CardId = comment.CardId,
                UserId = comment.UserId,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,

                UserName = user?.Name ?? "Unknown",
                UserAvatar = user?.AvatarUrl ?? string.Empty
            };

            await _realtimeService.SendToBoardAsync(
                boardId: list.BoardId,
                method: CommentEvents.Added,
                payload: dto
            );

            return Result<CommentWithUserDto>.Success(dto);
        }
    }
}
