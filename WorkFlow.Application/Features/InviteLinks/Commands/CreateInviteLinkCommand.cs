using FluentValidation;
using MediatR;
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Application.Features.InviteLinks.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.InviteLinks.Commands
{
    public record CreateInviteLinkCommand(CreateInviteLinkDto Request) : IRequest<Result<InviteLinkDto>>;
    public class CreateInviteLinkCommandValidator : AbstractValidator<CreateInviteLinkCommand>
    {
        public CreateInviteLinkCommandValidator()
        {
            RuleFor(x => x.Request).NotNull();
            RuleFor(x => x.Request.Type).IsInEnum();
            RuleFor(x => x.Request.TargetId).NotEmpty();
        }
    }
    public class CreateInviteLinkCommandHandler : IRequestHandler<CreateInviteLinkCommand, Result<InviteLinkDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<InviteLink, Guid> _repository;

        public CreateInviteLinkCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _repository = unitOfWork.GetRepository<InviteLink, Guid>();
        }

        public async Task<Result<InviteLinkDto>> Handle(CreateInviteLinkCommand request, CancellationToken cancellationToken)
        {
            var token = Guid.NewGuid().ToString("N");
            var workSpaceId = request.Request.Type == InviteLinkType.WorkSpace ? request.Request.TargetId : (Guid?)null;
            var boardId = request.Request.Type == InviteLinkType.Board ? request.Request.TargetId : (Guid?)null;

            var link = InviteLink.Create(
                request.Request.Type,
                token,
                workSpaceId,
                boardId,
                request.Request.ExpiredAt
            );

            await _repository.AddAsync(link);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<InviteLinkDto>.Success(new InviteLinkDto
            {
                Id = link.Id,
                Token = link.Token,
                Type = link.Type,
                Status = link.Status,
                TargetId = request.Request.TargetId,
                ExpiredAt = link.ExpiredAt
            });
        }
    }
}
