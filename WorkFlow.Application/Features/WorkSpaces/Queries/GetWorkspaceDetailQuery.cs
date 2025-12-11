using AutoMapper;
using MediatR;
using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Features.WorkSpaces.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.WorkSpaces.Queries
{
    public record GetWorkspaceDetailQuery(Guid WorkspaceId)
    : IRequest<Result<WorkSpaceDetailDto>>;
    public class GetWorkspaceDetailQueryHandler
        : IRequestHandler<GetWorkspaceDetailQuery, Result<WorkSpaceDetailDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;

        public GetWorkspaceDetailQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<Result<WorkSpaceDetailDto>> Handle(GetWorkspaceDetailQuery request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<WorkSpaceDetailDto>.Failure("Không xác định người dùng.");

            var userId = _currentUser.UserId.Value;

            var workspaceRepo = _unitOfWork.GetRepository<WorkSpace, Guid>();
            var memberRepo = _unitOfWork.GetRepository<WorkspaceMember, Guid>();
            var boardRepo = _unitOfWork.GetRepository<Board, Guid>();

            var workspace = await workspaceRepo.GetByIdAsync(request.WorkspaceId);
            if (workspace == null)
                return Result<WorkSpaceDetailDto>.Failure("Workspace không tồn tại.");

            var membership = await memberRepo.FirstOrDefaultAsync(
                m => m.WorkSpaceId == workspace.Id && m.UserId == userId);

            if (membership == null)
                throw new ForbiddenAccessException("Bạn không có quyền truy cập Workspace này.");

            var dto = _mapper.Map<WorkSpaceDetailDto>(workspace);

            dto.Role = membership.Role.ToString();

            var members = await memberRepo.FindAsync(m => m.WorkSpaceId == workspace.Id);
            dto.TotalMembers = members.Count;

            var boards = await boardRepo.FindAsync(b => b.WorkSpaceId == workspace.Id && !b.IsArchived);
            dto.TotalBoards = boards.Count;

            return Result<WorkSpaceDetailDto>.Success(dto);
        }
    }
}
