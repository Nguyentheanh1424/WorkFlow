using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
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
    public class UploadAttachmentCommand : IRequest<Result<Guid>>
    {
        public Guid CardId { get; private set; }
        public IFormFile File { get; set; } = default!;

        public void SetCardId(Guid cardId)
        {
            CardId = cardId;
        }
    }

    public class UploadAttachmentCommandValidator
        : AbstractValidator<UploadAttachmentCommand>
    {
        private static readonly string[] AllowedContentTypes =
        {
            // Images
            "image/jpeg",
            "image/png",
            "image/webp",
            "image/gif",

            // Documents
            "application/pdf",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.ms-excel",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        };

        public UploadAttachmentCommandValidator()
        {
            RuleFor(x => x.CardId)
                .NotEmpty()
                .WithMessage("CardId không được để trống.");

            RuleFor(x => x.File)
                .NotNull()
                .WithMessage("File không được để trống.");

            RuleFor(x => x.File.Length)
                .GreaterThan(0)
                .WithMessage("File rỗng.")
                .LessThanOrEqualTo(20 * 1024 * 1024)
                .WithMessage("Dung lượng file tối đa là 20MB.");

            RuleFor(x => x.File.ContentType)
                .Must(type => AllowedContentTypes.Contains(type))
                .WithMessage("Chỉ cho phép upload file tài liệu hoặc hình ảnh.");
        }
    }


    public class UploadAttachmentCommandHandler
        : IRequestHandler<UploadAttachmentCommand, Result<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Card, Guid> _cardRepository;
        private readonly IRepository<Attachment, Guid> _attachmentRepository;
        private readonly IRepository<List, Guid> _listdRepository;

        private readonly IPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IFileStorageService _fileStorage;
        private readonly IRealtimeService _realtime;
        private readonly IMapper _mapper;

        public UploadAttachmentCommandHandler(
            IUnitOfWork unitOfWork,
            IPermissionService permission,
            ICurrentUserService currentUser,
            IFileStorageService fileStorage,
            IRealtimeService realtime,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _cardRepository = unitOfWork.GetRepository<Card, Guid>();
            _attachmentRepository = unitOfWork.GetRepository<Attachment, Guid>();
            _listdRepository = unitOfWork.GetRepository<List, Guid>();

            _permission = permission;
            _currentUser = currentUser;
            _fileStorage = fileStorage;
            _realtime = realtime;
            _mapper = mapper;
        }

        public async Task<Result<Guid>> Handle(
            UploadAttachmentCommand request,
            CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<Guid>.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var card = await _cardRepository.GetByIdAsync(request.CardId)
                ?? throw new NotFoundException("Card không tồn tại.");

            var list = await _listdRepository.GetByIdAsync(card.ListId)
                ?? throw new NotFoundException("Danh sách không tồn tại.");

            var boardId = list.BoardId;

            await _permission.Card.EnsureCanEditAsync(card.Id, userId);

            string fileUrl;
            using (var stream = request.File.OpenReadStream())
            {
                fileUrl = await _fileStorage.UploadAsync(
                    stream,
                    request.File.FileName,
                    request.File.ContentType,
                    cancellationToken
                );
            }

            var attachment = Attachment.Create(
                cardId: card.Id,
                userId: userId,
                fileType: request.File.ContentType ?? "application/octet-stream",
                fileUrl: fileUrl,
                fileName: request.File.FileName
            );

            await _attachmentRepository.AddAsync(attachment);
            await _unitOfWork.SaveChangesAsync();

            var payload = _mapper.Map<AttachmentDto>(attachment);

            await _realtime.SendToBoardAsync(
                boardId: boardId,   
                method: BoardEvents.UploadAttachment,
                payload
            );

            return Result<Guid>.Success(attachment.Id);
        }
    }
}
