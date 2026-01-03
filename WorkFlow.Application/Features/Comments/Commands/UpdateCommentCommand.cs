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
    public class UpdateCommentCommand : IRequest<Result<bool>>
    {
        public Guid CommentId { get; private set; }
        public string Content { get; set; } = null!;

        public void SetCommentId(Guid commentId)
        {
            CommentId = commentId;
        }
    }

    public class UpdateCommentCommandValidator
        : AbstractValidator<UpdateCommentCommand>
    {
        public UpdateCommentCommandValidator()
        {
            RuleFor(x => x.CommentId)
                .NotEmpty()
                .WithMessage("CommentId không được để trống.");

            RuleFor(x => x.Content)
                .NotEmpty()
                .WithMessage("Nội dung comment không được để trống.")
                .MaximumLength(2000)
                .WithMessage("Nội dung comment tối đa 2000 ký tự.");
        }
    }

    public class UpdateCommentCommandHandler
        : IRequestHandler<UpdateCommentCommand, Result<bool>>
    {
        private static readonly TimeSpan EditTimeLimit = TimeSpan.FromSeconds(36);

        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Comment, Guid> _commentRepository;
        private readonly IRepository<Card, Guid> _cardRepository;
        private readonly IRepository<List, Guid> _listRepository;

        private readonly IPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IRealtimeService _realtime;
        private readonly IMapper _mapper;

        public UpdateCommentCommandHandler(
            IUnitOfWork unitOfWork,
            IPermissionService permission,
            ICurrentUserService currentUser,
            IRealtimeService realtime,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _commentRepository = unitOfWork.GetRepository<Comment, Guid>();
            _cardRepository = unitOfWork.GetRepository<Card, Guid>();
            _listRepository = unitOfWork.GetRepository<List, Guid>();

            _permission = permission;
            _currentUser = currentUser;
            _realtime = realtime;
            _mapper = mapper;
        }

        public async Task<Result<bool>> Handle(
            UpdateCommentCommand request,
            CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<bool>.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var comment = await _commentRepository.GetByIdAsync(request.CommentId)
                ?? throw new NotFoundException("Comment không tồn tại.");

            // RULE 36 GIÂY
            if (DateTime.UtcNow - comment.CreatedAt > EditTimeLimit)
            {
                return Result<bool>.Failure(
                    "Chỉ có thể chỉnh sửa comment trong vòng 36 giây sau khi tạo."
                );
            }

            var card = await _cardRepository.GetByIdAsync(comment.CardId)
                ?? throw new NotFoundException("Card không tồn tại.");

            if (comment.UserId != userId)
            {
                throw new ForbiddenAccessException("Bạn không có quyền chỉnh sửa comment !");
            }

            comment.Edit(request.Content.Trim());

            await _unitOfWork.SaveChangesAsync();

            var list = await _listRepository.GetByIdAsync(card.ListId)
                ?? throw new NotFoundException("Danh sách không tồn tại.");

            var dto = _mapper.Map<CommentDto>(comment);

            await _realtime.SendToBoardAsync(
                boardId: list.BoardId,
                "BoardNotification",
                new
                {
                    Action = CommentEvents.Updated,
                    Data = dto
                }

            );

            return Result<bool>.Success(true);
        }
    }
}
