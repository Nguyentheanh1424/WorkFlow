using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.BoardMembers.Dtos
{
    public class BoardMemberDto
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public BoardRole? Role { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}
