namespace WorkFlow.Application.Features.InviteLinks.Queries
{
    using AutoMapper;
    using FluentValidation;
    using global::WorkFlow.Application.Common.Interfaces.Auth;
    using global::WorkFlow.Application.Common.Interfaces.Repositories;
    using global::WorkFlow.Application.Common.Interfaces.Services;
    using global::WorkFlow.Application.Features.InviteLinks.Dtos;
    using global::WorkFlow.Domain.Common;
    using global::WorkFlow.Domain.Entities;
    using global::WorkFlow.Domain.Enums;
    using MediatR;

    namespace WorkFlow.Application.Features.InviteLinks.Queries
    {
        public record GetInviteLinksQuery(
            InviteLinkType Type,
            Guid TargetId
        ) : IRequest<Result<List<InviteLinkDto>>>;

        public class GetInviteLinksQueryValidator
            : AbstractValidator<GetInviteLinksQuery>
        {
            public GetInviteLinksQueryValidator()
            {
                RuleFor(x => x.TargetId)
                    .NotEmpty()
                    .WithMessage("TargetId không được để trống.");
            }
        }

        public class GetInviteLinksQueryHandler
            : IRequestHandler<GetInviteLinksQuery, Result<List<InviteLinkDto>>>
        {
            private readonly IRepository<InviteLink, Guid> _inviteLinkRepository;
            private readonly ICurrentUserService _currentUser;
            private readonly IPermissionService _permission;
            private readonly IMapper _mapper;

            public GetInviteLinksQueryHandler(
                IUnitOfWork unitOfWork,
                ICurrentUserService currentUser,
                IPermissionService permission,
                IMapper mapper)
            {
                _inviteLinkRepository = unitOfWork.GetRepository<InviteLink, Guid>();
                _currentUser = currentUser;
                _permission = permission;
                _mapper = mapper;
            }

            public async Task<Result<List<InviteLinkDto>>> Handle(
                GetInviteLinksQuery request,
                CancellationToken cancellationToken)
            {
                if (_currentUser.UserId == null)
                    return Result<List<InviteLinkDto>>.Failure("Không xác định được người dùng.");

                var userId = _currentUser.UserId.Value;

                if (request.Type == InviteLinkType.WorkSpace)
                {
                    await _permission.Workspace.EnsureAdminAsync(request.TargetId, userId);
                }
                else if (request.Type == InviteLinkType.Board)
                {
                    await _permission.Board.EnsureOwnerAsync(request.TargetId, userId);
                }
                else
                {
                    return Result<List<InviteLinkDto>>.Failure("InviteLinkType không hợp lệ.");
                }

                var inviteLinks = await _inviteLinkRepository.FindAsync(
                    x => x.Type == request.Type && x.TargetId == request.TargetId
                );

                var dtos = inviteLinks
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(x => _mapper.Map<InviteLinkDto>(x))
                    .ToList();

                return Result<List<InviteLinkDto>>.Success(dtos);
            }
        }
    }

}
