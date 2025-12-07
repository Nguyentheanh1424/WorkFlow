using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Enums;
using Task = System.Threading.Tasks.Task;

namespace WorkFlow.Application.Common.Services
{
    public class BoardPermissionService : IBoardPermissionService
    {
        private readonly IRepository<BoardMember, Guid> _boardMemberRepository;
        private readonly IRepository<Board, Guid> _boardRepository;
        private readonly IWorkspacePermissionService _workspacePermission;

        public BoardPermissionService(
            IUnitOfWork unitOfWork,
            IWorkspacePermissionService workspacePermission)
        {
            _boardMemberRepository = unitOfWork.GetRepository<BoardMember, Guid>();
            _boardRepository = unitOfWork.GetRepository<Board, Guid>();
            _workspacePermission = workspacePermission;
        }

        public async Task<BoardRole?> GetRoleAsync(Guid boardId, Guid userId)
        {
            var member = await _boardMemberRepository.FirstOrDefaultAsync(
                x => x.BoardId == boardId && x.UserId == userId
            );

            return member?.Role;
        }

        public async Task EnsureViewerAsync(Guid boardId, Guid userId)
        {
            var board = await _boardRepository.GetByIdAsync(boardId);
            if (board == null)
                throw new NotFoundException("Board không tồn tại.");

            await _workspacePermission.EnsureMemberAsync(board.WorkspaceId, userId);

            var role = await GetRoleAsync(boardId, userId);

            if (role == null)
                throw new ForbiddenAccessException("Bạn không có quyền xem Board này.");
        }

        public async Task EnsureEditorAsync(Guid boardId, Guid userId)
        {
            var board = await _boardRepository.GetByIdAsync(boardId);
            if (board == null)
                throw new NotFoundException("Board không tồn tại.");

            await _workspacePermission.EnsureMemberAsync(board.WorkspaceId, userId);

            var role = await GetRoleAsync(boardId, userId);

            if (role is null or BoardRole.Viewer)
                throw new ForbiddenAccessException("Bạn không có quyền chỉnh sửa Board.");
        }

        public async Task EnsureOwnerAsync(Guid boardId, Guid userId)
        {
            var board = await _boardRepository.GetByIdAsync(boardId);
            if (board == null)
                throw new NotFoundException("Board không tồn tại.");

            await _workspacePermission.EnsureMemberAsync(board.WorkspaceId, userId);

            var role = await GetRoleAsync(boardId, userId);

            if (role != BoardRole.Owner)
                throw new ForbiddenAccessException("Bạn không có quyền quản trị Board.");
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
}
