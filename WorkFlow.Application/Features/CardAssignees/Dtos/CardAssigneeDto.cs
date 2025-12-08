using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow.Application.Common.Mappings;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.CardAssignees.Dtos
{
    public class CardAssigneeDto : IMapFrom<CardAssignee>
    {
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
