using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow.Application.Common.Mappings;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Lists.Dtos
{
    public class ListDto : IMapFrom<List>
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public int Position { get; set; }
        public bool IsArchived { get; set; }
    }
}
