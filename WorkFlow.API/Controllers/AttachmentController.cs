//using MediatR;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Swashbuckle.AspNetCore.Annotations;

//namespace WorkFlow.API.Controllers
//{
//    [ApiController]
//    [Route("cards/{cardId:guid}/attachments")]
//    [Authorize]
//    public class AttachmentController : ControllerBase
//    {
//        private readonly IMediator _mediator;

//        public AttachmentController(IMediator mediator)
//        {
//            _mediator = mediator;
//        }

//        // GET: cards/{cardId}/attachments
//        [HttpGet]
//        [SwaggerOperation(
//            Summary = "Lấy danh sách file đính kèm của Card",
//            Description = "Trả về toàn bộ danh sách file được upload vào Card."
//        )]
//        public async Task<IActionResult> GetAttachments(Guid cardId)
//        {
//            var result = await _mediator.Send(new GetAttachmentsQuery(cardId));
//            return result.IsSuccess ? Ok(result) : BadRequest(result);
//        }

//        // POST: cards/{cardId}/attachments
//        [HttpPost]
//        [SwaggerOperation(
//            Summary = "Tải file đính kèm lên Card",
//            Description = "Upload file lên và gắn file đó vào Card."
//        )]
//        public async Task<IActionResult> UploadAttachment(
//            Guid cardId,
//            [FromForm] UploadAttachmentCommand command)
//        {
//            command.SetCardId(cardId);
//            var result = await _mediator.Send(command);
//            return result.IsSuccess ? Ok(result) : BadRequest(result);
//        }

//        // DELETE: attachments/{attachmentId}
//        [HttpDelete("/attachments/{attachmentId:guid}")]
//        [SwaggerOperation(
//            Summary = "Xóa file đính kèm",
//            Description = "Xóa 1 file đính kèm theo attachmentId."
//        )]
//        public async Task<IActionResult> DeleteAttachment(Guid attachmentId)
//        {
//            var result = await _mediator.Send(new DeleteAttachmentCommand(attachmentId));
//            return result.IsSuccess ? Ok(result) : BadRequest(result);
//        }
//    }
//}
