using AutoMapper;
using MediatR;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Application.Features.WorkSpaces.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.WorkSpaces.Queries
{
    public record GetWorkspacesQuery(string? Search)
    : IRequest<Result<List<WorkSpaceDto>>>;


    public class GetWorkspacesQueryHandler
        : IRequestHandler<GetWorkspacesQuery, Result<List<WorkSpaceDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;

        public GetWorkspacesQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<Result<List<WorkSpaceDto>>> Handle(GetWorkspacesQuery request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<List<WorkSpaceDto>>.Failure("Không xác định người dùng.");

            var userId = _currentUser.UserId.Value;

            var workspaceRepo = _unitOfWork.GetRepository<WorkSpace, Guid>();
            var memberRepo = _unitOfWork.GetRepository<WorkspaceMember, Guid>();

            var memberships = await memberRepo.FindAsync(m => m.UserId == userId);

            if (!memberships.Any())
                return Result<List<WorkSpaceDto>>.Success(new List<WorkSpaceDto>());

            var workspaceIds = memberships.Select(m => m.WorkSpaceId).ToList();

            var workspaces = await workspaceRepo.FindAsync(ws => workspaceIds.Contains(ws.Id));

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim().ToLower();
                workspaces = workspaces
                    .Where(ws =>
                        (ws.Name ?? "").ToLower().Contains(keyword) ||
                        (ws.Description ?? "").ToLower().Contains(keyword))
                    .ToList();
            }

            var memberLookup = memberships.ToDictionary(m => m.WorkSpaceId, m => m.Role.ToString());

            var dtos = workspaces
                .OrderByDescending(ws => ws.UpdatedAt)
                .Select(ws =>
                {
                    var dto = _mapper.Map<WorkSpaceDto>(ws);
                    dto.Role = memberLookup.ContainsKey(ws.Id) ? memberLookup[ws.Id] : "";
                    return dto;
                })
                .ToList();

            return Result<List<WorkSpaceDto>>.Success(dtos);
        }
    }
}
