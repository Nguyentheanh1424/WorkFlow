using AutoMapper;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Application.Features.Attachments.Dtos;
using WorkFlow.Application.Features.CardAssignees.Dtos;
using WorkFlow.Application.Features.Cards.Dtos;
using WorkFlow.Application.Features.SubTasks.Dtos;
using WorkFlow.Application.Features.Tasks.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.Cards.Queries
{
    public record GetCardDetailQuery(Guid CardId)
        : IRequest<Result<CardDetailDto>>;

    public class GetCardDetailQueryValidator : AbstractValidator<GetCardDetailQuery>
    {
        public GetCardDetailQueryValidator()
        {
            RuleFor(x => x.CardId).NotEmpty();
        }
    }

    public class GetCardDetailQueryHandler
        : IRequestHandler<GetCardDetailQuery, Result<CardDetailDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBoardPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;

        public GetCardDetailQueryHandler(
            IUnitOfWork unitOfWork,
            IBoardPermissionService permission,
            ICurrentUserService currentUser,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _permission = permission;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<Result<CardDetailDto>> Handle(GetCardDetailQuery request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<CardDetailDto>.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var cardRepo = _unitOfWork.GetRepository<Card, Guid>();
            var listRepo = _unitOfWork.GetRepository<List, Guid>();
            var boardRepo = _unitOfWork.GetRepository<Board, Guid>();
            var assigneeRepo = _unitOfWork.GetRepository<CardAssignee, Guid>();
            var attachmentRepo = _unitOfWork.GetRepository<Attachment, Guid>();
            var taskRepo = _unitOfWork.GetRepository<Domain.Entities.Task, Guid>();
            var subTaskRepo = _unitOfWork.GetRepository<SubTask, Guid>();
            var subTaskAssigneeRepo = _unitOfWork.GetRepository<SubTaskAssignee, Guid>();

            var card = await cardRepo.GetByIdAsync(request.CardId);
            if (card == null)
                return Result<CardDetailDto>.Failure("Card không tồn tại.");

            var list = await listRepo.GetByIdAsync(card.ListId)
                ?? throw new NotFoundException("List không tồn tại.");

            var board = await boardRepo.GetByIdAsync(list.BoardId)
                ?? throw new NotFoundException("Board không tồn tại.");

            await _permission.EnsureViewerAsync(board.Id, userId);

            var dto = _mapper.Map<CardDetailDto>(card);

            var assignees = await assigneeRepo.FindAsync(a => a.CardId == card.Id);
            dto.Assignees = _mapper.Map<List<CardAssigneeDto>>(assignees);

            var attachments = await attachmentRepo.FindAsync(a => a.CardId == card.Id);
            dto.Attachments = _mapper.Map<List<AttachmentDto>>(attachments);

            var tasks = await taskRepo.FindAsync(t => t.CardId == card.Id);
            var taskDtos = new List<TaskDto>();

            foreach (var task in tasks.OrderBy(t => t.Position))
            {
                var taskDto = _mapper.Map<TaskDto>(task);

                var subTasks = await subTaskRepo.FindAsync(st => st.TaskId == task.Id);

                taskDto.SubTasks = subTasks
                    .OrderBy(st => st.Position)
                    .Select(st => _mapper.Map<SubTaskDto>(st))
                    .ToList();

                taskDto.TotalSubTasks = taskDto.SubTasks.Count;
                taskDto.CompletedSubTasks = taskDto.SubTasks.Count(x => x.Status == JobStatus.Done);
                taskDto.Progress = taskDto.TotalSubTasks == 0
                    ? 0
                    : (double)taskDto.CompletedSubTasks / taskDto.TotalSubTasks;

                dto.Tasks.Add(taskDto);
            }


            return Result<CardDetailDto>.Success(dto);
        }
    }
}
