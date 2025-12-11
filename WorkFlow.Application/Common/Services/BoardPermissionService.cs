using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Enums;
using Task = System.Threading.Tasks.Task;

public class BoardPermissionService : IBoardPermissionService
{
    private readonly IRepository<BoardMember, Guid> _boardMemberRepository;
    private readonly IRepository<Board, Guid> _boardRepository;
    private readonly IWorkSpacePermissionService _workspacePermission;

    public BoardPermissionService(
        IUnitOfWork unitOfWork,
        IWorkSpacePermissionService workspacePermission)
    {
        _boardMemberRepository = unitOfWork.GetRepository<BoardMember, Guid>();
        _boardRepository = unitOfWork.GetRepository<Board, Guid>();
        _workspacePermission = workspacePermission;
    }

    private async Task<bool> IsWorkspaceAdminOrOwner(Guid workspaceId, Guid userId)
    {
        var role = await _workspacePermission.GetRoleAsync(workspaceId, userId);
        return role is WorkSpaceRole.Admin or WorkSpaceRole.Owner;
    }

    private int Rank(BoardRole role) => role switch
    {
        BoardRole.Owner => 3,
        BoardRole.Editor => 2,
        BoardRole.Viewer => 1,
        _ => 0
    };

    public async Task<BoardRole?> GetRoleAsync(Guid boardId, Guid userId)
    {
        var member = await _boardMemberRepository.FirstOrDefaultAsync(
            x => x.BoardId == boardId && x.UserId == userId
        );
        return member?.Role;
    }

    public async Task EnsureViewerAsync(Guid boardId, Guid userId)
    {
        var board = await _boardRepository.GetByIdAsync(boardId)
            ?? throw new NotFoundException("Board không tồn tại.");

        // Workspace Owner/Admin luôn pass
        if (await IsWorkspaceAdminOrOwner(board.WorkSpaceId, userId))
            return;

        await _workspacePermission.EnsureMemberAsync(board.WorkSpaceId, userId);

        var role = await GetRoleAsync(boardId, userId);
        if (role == null)
            throw new ForbiddenAccessException("Bạn không có quyền xem Board này.");
    }

    public async Task EnsureEditorAsync(Guid boardId, Guid userId)
    {
        var board = await _boardRepository.GetByIdAsync(boardId)
            ?? throw new NotFoundException("Board không tồn tại.");

        if (await IsWorkspaceAdminOrOwner(board.WorkSpaceId, userId))
            return;

        await _workspacePermission.EnsureMemberAsync(board.WorkSpaceId, userId);

        var role = await GetRoleAsync(boardId, userId);

        if (role is null or BoardRole.Viewer)
            throw new ForbiddenAccessException("Bạn không có quyền chỉnh sửa Board.");
    }

    public async Task EnsureOwnerAsync(Guid boardId, Guid userId)
    {
        var board = await _boardRepository.GetByIdAsync(boardId)
            ?? throw new NotFoundException("Board không tồn tại.");

        if (await IsWorkspaceAdminOrOwner(board.WorkSpaceId, userId))
            return;

        await _workspacePermission.EnsureMemberAsync(board.WorkSpaceId, userId);

        var role = await GetRoleAsync(boardId, userId);

        if (role != BoardRole.Owner)
            throw new ForbiddenAccessException("Bạn không có quyền quản trị Board.");
    }

    public async Task EnsureCanAssignRoleAsync(Guid boardId, Guid currentUserId, BoardRole newRole)
    {
        var board = await _boardRepository.GetByIdAsync(boardId)
            ?? throw new NotFoundException("Board không tồn tại.");

        // Workspace Owner/Admin có quyền assign mọi role
        if (await IsWorkspaceAdminOrOwner(board.WorkSpaceId, currentUserId))
            return;

        var currentRole = await GetRoleAsync(boardId, currentUserId)
            ?? throw new ForbiddenAccessException("Bạn không thuộc Board.");

        if (Rank(newRole) > Rank(currentRole))
            throw new ForbiddenAccessException("Bạn không thể gán role cao hơn quyền hiện tại của bạn.");
    }

    public async Task EnsureCanModifyMemberRoleAsync(Guid boardId, Guid currentUserId, Guid targetUserId)
    {
        var board = await _boardRepository.GetByIdAsync(boardId)
            ?? throw new NotFoundException("Board không tồn tại.");

        // Workspace Owner/Admin có quyền chỉnh sửa mọi thành viên
        if (await IsWorkspaceAdminOrOwner(board.WorkSpaceId, currentUserId))
            return;

        var currentRole = await GetRoleAsync(boardId, currentUserId)
            ?? throw new ForbiddenAccessException("Bạn không thuộc Board.");

        var targetRole = await GetRoleAsync(boardId, targetUserId);

        // nếu target chưa có trong board => không cần check
        if (targetRole == null)
            return;

        if (Rank(targetRole.Value) > Rank(currentRole))
            throw new ForbiddenAccessException("Bạn không thể chỉnh sửa thành viên có quyền cao hơn bạn.");
    }

    public async Task<bool> IsLastOwnerAsync(Guid boardId, Guid userId)
    {
        var role = await GetRoleAsync(boardId, userId);
        if (role != BoardRole.Owner)
            return false;

        var ownerCount = await _boardMemberRepository.CountAsync(
            x => x.BoardId == boardId && x.Role == BoardRole.Owner
        );

        return ownerCount <= 1;
    }
}
