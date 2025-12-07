using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkFlow.Application.Common.Interfaces.Services
{
    public interface IPermissionService
    {
        IWorkspacePermissionService Workspace { get; }
        IBoardPermissionService Board { get; }
        ICardPermissionService Card { get; }
    }
}
