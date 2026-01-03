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
    public record DeleteCommentCommand(Guid CommentId)
        : IRequest<Result<bool>>;

    public class DeleteCommentCommandValidator
        : AbstractValidator<DeleteCommentCommand>
    {
        public DeleteCommentCommandValidator()
        {
            RuleFor(x => x.CommentId)
                .NotEmpty()
                .WithMessage("CommentId không được để trống.");
        }
    }

    public class DeleteCommentCommandHandler
        : IRequestHandler<DeleteCommentCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Comment, Guid> _commentRepository;
        private readonly IRepository<Card, Guid> _cardRepository;
        private readonly IRepository<List, Guid> _listRepository;

        private readonly IPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IRealtimeService _realtime;
        private readonly IMapper _mapper;

        public DeleteCommentCommandHandler(
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
            DeleteCommentCommand request,
            CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<bool>.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var comment = await _commentRepository.GetByIdAsync(request.CommentId)
                ?? throw new NotFoundException("Comment không tồn tại.");

            var card = await _cardRepository.GetByIdAsync(comment.CardId)
                ?? throw new NotFoundException("Card không tồn tại.");

            if (comment.UserId != userId)
            {
                throw new ForbiddenAccessException("Bạn không có quyền xóa bình luận này.");
            }

            await _commentRepository.DeleteAsync(comment);
            await _unitOfWork.SaveChangesAsync();

            var list = await _listRepository.GetByIdAsync(card.ListId)
                ?? throw new NotFoundException("Danh sách không tồn tại.");

            var dto = _mapper.Map<CommentDto>(comment);

            await _realtime.SendToBoardAsync(
                boardId: list.BoardId,
                "BoardNotification",
                new
                {
                    Action = CommentEvents.Deleted,
                    Data = dto
                }

            );

            return Result<bool>.Success(true);
        }
    }
}
