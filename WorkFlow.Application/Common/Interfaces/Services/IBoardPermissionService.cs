using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Common.Interfaces.Services
{
    public interface IBoardPermissionService
    {
        Task<BoardRole?> GetRoleAsync(Guid boardId, Guid userId);

        Task EnsureViewerAsync(Guid boardId, Guid userId);
        Task EnsureEditorAsync(Guid boardId, Guid userId);
        Task EnsureOwnerAsync(Guid boardId, Guid userId);

        Task EnsureCanAssignRoleAsync(Guid boardId, Guid currentUserId, BoardRole newRole);
        Task EnsureCanModifyMemberRoleAsync(Guid boardId, Guid currentUserId, Guid targetUserId);

        Task<bool> IsLastOwnerAsync(Guid boardId, Guid userId);
    }
}
