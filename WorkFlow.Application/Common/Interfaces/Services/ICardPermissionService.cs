using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkFlow.Application.Common.Interfaces.Services
{
    public interface ICardPermissionService
    {
        Task EnsureCanViewAsync(Guid cardId, Guid userId);
        Task EnsureCanEditAsync(Guid cardId, Guid userId);
        Task EnsureCanAssignAsync(Guid cardId, Guid userId);
        Task EnsureCanCommentAsync(Guid cardId, Guid userId);
        Task EnsureCanDeleteAsync(Guid cardId, Guid userId);
    }
}
