using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkFlow.Domain.Enums
{
    public enum BoardRole
    {
        None,
        Viewer = 0,
        Editor = 1,
        Owner = 2
    }
}
