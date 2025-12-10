using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow.Application.Common.Interfaces.Services;

namespace WorkFlow.Application.Common.Services
{
    public class PermissionService : IPermissionService
    {
        public IWorkSpacePermissionService Workspace { get; }
        public IBoardPermissionService Board { get; }
        public ICardPermissionService Card { get; }

        public PermissionService(
            IWorkSpacePermissionService workspacePermissionService,
            IBoardPermissionService boardPermissionService,
            ICardPermissionService cardPermissionService)
        {
            Workspace = workspacePermissionService;
            Board = boardPermissionService;
            Card = cardPermissionService;
        }
    }

}
