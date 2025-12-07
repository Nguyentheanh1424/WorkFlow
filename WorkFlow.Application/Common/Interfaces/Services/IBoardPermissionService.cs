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

        Task<bool> IsLastOwnerAsync(Guid boardId, Guid userId);
    }
}
