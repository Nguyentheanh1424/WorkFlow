using AutoMapper;
using FluentValidation;
using MediatR;
using WorkFlow.Application.Common.Constants.EventNames;
using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Application.Features.Attachments.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Attachments.Commands
{
    public record DeleteAttachmentCommand(Guid AttachmentId)
        : IRequest<Result<bool>>;

    public class DeleteAttachmentCommandValidator
        : AbstractValidator<DeleteAttachmentCommand>
    {
        public DeleteAttachmentCommandValidator()
        {
            RuleFor(x => x.AttachmentId)
                .NotEmpty()
                .WithMessage("AttachmentId không được để trống.");
        }
    }

    public class DeleteAttachmentCommandHandler
        : IRequestHandler<DeleteAttachmentCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Attachment, Guid> _attachmentRepository;
        private readonly IRepository<Card, Guid> _cardRepository;
        private readonly IRepository<List, Guid> _listRepository;

        private readonly IPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IFileStorageService _fileStorage;
        private readonly IRealtimeService _realtime;
        private readonly IMapper _mapper;

        public DeleteAttachmentCommandHandler(
            IUnitOfWork unitOfWork,
            IPermissionService permission,
            ICurrentUserService currentUser,
            IFileStorageService fileStorage,
            IRealtimeService realtime,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _attachmentRepository = unitOfWork.GetRepository<Attachment, Guid>();
            _cardRepository = unitOfWork.GetRepository<Card, Guid>();
            _listRepository = unitOfWork.GetRepository<List, Guid>();

            _permission = permission;
            _currentUser = currentUser;
            _fileStorage = fileStorage;
            _realtime = realtime;
            _mapper = mapper;
        }

        public async Task<Result<bool>> Handle(
            DeleteAttachmentCommand request,
            CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<bool>.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var attachment = await _attachmentRepository.GetByIdAsync(request.AttachmentId)
                ?? throw new NotFoundException("Attachment không tồn tại.");

            var card = await _cardRepository.GetByIdAsync(attachment.CardId)
                ?? throw new NotFoundException("Card không tồn tại.");

            var list = await _listRepository.GetByIdAsync(card.ListId)
                ?? throw new NotFoundException("Danh sách không tồn tại.");

            var boardId = list.BoardId;

            if (attachment.UserId != userId)
            {
                await _permission.Card.EnsureCanEditAsync(card.Id, userId);
            }

            await _fileStorage.DeleteAsync(attachment.FileUrl, cancellationToken);

            await _attachmentRepository.DeleteAsync(attachment);
            await _unitOfWork.SaveChangesAsync();

            var payload = _mapper.Map<AttachmentDto>(attachment);

            await _realtime.SendToBoardAsync(
                boardId: boardId,
                method: BoardEvents.RemoveAttachment,
                payload
            );

            return Result<bool>.Success(true);
        }
    }
}
