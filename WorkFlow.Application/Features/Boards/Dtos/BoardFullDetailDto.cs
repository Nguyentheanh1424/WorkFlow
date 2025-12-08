using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow.Application.Features.BoardMembers.Dtos;
using WorkFlow.Application.Features.Cards.Dtos;
using WorkFlow.Application.Features.Lists.Dtos;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.Boards.Dtos
{
    public class BoardFullDetailDto
    {
        public BoardDto? Board { get; set; }
        public List<ListDto>? Lists { get; set; }
        public List<CardDto>? Cards { get; set; }
        public List<BoardMemberDto>? Members { get; set; }
        public BoardRole CurrentUserRole { get; set; }
    }

}
