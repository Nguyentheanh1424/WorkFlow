using AutoMapper;
using FluentValidation;
using MediatR;
using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Application.Features.Attachments.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Attachments.Queries
{
    public record GetAttachmentsQuery(Guid CardId)
        : IRequest<Result<List<AttachmentDto>>>;

    public class GetAttachmentsQueryValidator
        : AbstractValidator<GetAttachmentsQuery>
    {
        public GetAttachmentsQueryValidator()
        {
            RuleFor(x => x.CardId)
                .NotEmpty()
                .WithMessage("CardId không được để trống.");
        }
    }

    public class GetAttachmentsQueryHandler
        : IRequestHandler<GetAttachmentsQuery, Result<List<AttachmentDto>>>
    {
        private readonly IRepository<Attachment, Guid> _attachmentRepository;
        private readonly IRepository<Card, Guid> _cardRepository;

        private readonly IPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;

        public GetAttachmentsQueryHandler(
            IUnitOfWork unitOfWork,
            IPermissionService permission,
            ICurrentUserService currentUser,
            IMapper mapper)
        {
            _attachmentRepository = unitOfWork.GetRepository<Attachment, Guid>();
            _cardRepository = unitOfWork.GetRepository<Card, Guid>();

            _permission = permission;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<Result<List<AttachmentDto>>> Handle(
            GetAttachmentsQuery request,
            CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<List<AttachmentDto>>.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var card = await _cardRepository.GetByIdAsync(request.CardId)
                ?? throw new NotFoundException("Card không tồn tại.");

            await _permission.Card.EnsureCanViewAsync(card.Id, userId);

            var attachments = await _attachmentRepository.FindAsync(
                a => a.CardId == card.Id
            );

            var ordered = attachments
                .OrderByDescending(a => a.CreatedAt)
                .ToList();

            var dtos = _mapper.Map<List<AttachmentDto>>(ordered);

            return Result<List<AttachmentDto>>.Success(dtos);
        }
    }
}
