using AutoMapper;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow.Application.Common.Constants.EventNames;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Application.Features.WorkSpaces.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.WorkSpaces.Commands
{
    public record CreateWorkspaceCommand(CreateWorkspaceDto workSpace) : IRequest<Result<WorkSpaceDto>>;

    public class CreateWorkspaceCommandValidator : AbstractValidator<CreateWorkspaceCommand>
    {
        public CreateWorkspaceCommandValidator()
        {
            RuleFor(x => x.workSpace.Type)
                .IsInEnum()
                .WithMessage("Type không hợp lệ.");

            RuleFor(x => x.workSpace.Name)
                .NotEmpty().WithMessage("Tên không được để trống.")
                .MaximumLength(255).WithMessage("Tên WorkSpace không được vượt quá 255 ký tự.");
        }
    }

    public class CreateWorkspaceCommandHandler : IRequestHandler<CreateWorkspaceCommand, Result<WorkSpaceDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRealtimeService _realtimeService;
        private readonly ICurrentUserService _currentUser;
        private readonly IRepository<WorkSpace, Guid> _workSpaceRepository;
        private readonly IMapper _mapper;

        public CreateWorkspaceCommandHandler(
            IUnitOfWork unitOfWork,
            IRealtimeService realtimeService,
            ICurrentUserService currentUser,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _realtimeService = realtimeService;
            _currentUser = currentUser;
            _workSpaceRepository = unitOfWork.GetRepository<WorkSpace, Guid>();
            _mapper = mapper;
        }

        public async Task<Result<WorkSpaceDto>> Handle(CreateWorkspaceCommand request, CancellationToken cancellationToken)
        {
            var workSpace = WorkSpace.Create(request.workSpace.Name, request.workSpace.Type, request.workSpace.Background, request.workSpace.Description);

            await _workSpaceRepository.AddAsync(workSpace);

            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<WorkSpaceDto>(workSpace);

            if (_currentUser.UserId == null)
                return Result<WorkSpaceDto>.Failure("Không xác định được người dùng.");

            await _realtimeService.SendToUserAsync(
                _currentUser.UserId.Value,
                WorkspaceEvents.Create,
                dto);

            return Result<WorkSpaceDto>.Success(dto);
        }
    }
}
