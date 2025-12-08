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
using WorkFlow.Application.Features.Lists.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Lists.Commands
{
    public record UnarchiveListCommand(Guid ListId)
        : IRequest<Result<ListDto>>;

    public class UnarchiveListCommandValidator : AbstractValidator<UnarchiveListCommand>
    {
        public UnarchiveListCommandValidator()
        {
            RuleFor(x => x.ListId)
                .NotEmpty().WithMessage("ListId không được để trống.");
        }
    }

    public class UnarchiveListCommandHandler
        : IRequestHandler<UnarchiveListCommand, Result<ListDto>>
    {
        private readonly IRepository<List, Guid> _listRepository;
        private readonly IRepository<Board, Guid> _boardRepository;
        private readonly IBoardPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IRealtimeService _realtime;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UnarchiveListCommandHandler(
            IBoardPermissionService permission,
            ICurrentUserService currentUser,
            IRealtimeService realtime,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _listRepository = unitOfWork.GetRepository<List, Guid>();
            _boardRepository = unitOfWork.GetRepository<Board, Guid>();
            _permission = permission;
            _currentUser = currentUser;
            _realtime = realtime;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<ListDto>> Handle(UnarchiveListCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<ListDto>.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var list = await _listRepository.GetByIdAsync(request.ListId);
            if (list == null)
                return Result<ListDto>.Failure("List không tồn tại.");

            var board = await _boardRepository.GetByIdAsync(list.BoardId);
            if (board == null)
                return Result<ListDto>.Failure("Board không tồn tại.");

            await _permission.EnsureEditorAsync(board.Id, userId);

            list.Unarchive();

            await _listRepository.UpdateAsync(list);
            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<ListDto>(list);

            await _realtime.SendToBoardAsync(board.Id, ListEvents.Unarchived, dto);

            return Result<ListDto>.Success(dto);
        }
    }

}
