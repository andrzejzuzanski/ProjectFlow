using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlow.Core.DTOs;
using ProjectFlow.Core.Entities;
using ProjectFlow.Core.Interfaces;
using System.Security.Claims;

namespace ProjectFlow.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AttachmentsController : ControllerBase
    {
        private readonly IAttachmentRepository _attachmentRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<AttachmentsController> _logger;
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".txt", ".zip" };
        private readonly INotificationService _notificationService;

        public AttachmentsController(
            IAttachmentRepository attachmentRepository,
            ITaskRepository taskRepository,
            IMapper mapper,
            IWebHostEnvironment environment,
            ILogger<AttachmentsController> logger,
            INotificationService notificationService)
        {
            _attachmentRepository = attachmentRepository;
            _taskRepository = taskRepository;
            _mapper = mapper;
            _environment = environment;
            _logger = logger;
            _notificationService = notificationService;
        }

        // POST: api/attachments/upload/{taskId}
        [HttpPost("upload/{taskId}")]
        public async Task<ActionResult<AttachmentDto>> UploadAttachment(int taskId, IFormFile file)
        {
            try
            {
                // Validate task exists
                var task = await _taskRepository.GetByIdAsync(taskId);
                if (task == null)
                {
                    return NotFound($"Task with ID {taskId} not found");
                }

                // Validate file
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded");
                }

                if (file.Length > MaxFileSize)
                {
                    return BadRequest($"File size exceeds maximum allowed size of {MaxFileSize / 1024 / 1024}MB");
                }

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(extension))
                {
                    return BadRequest($"File type {extension} is not allowed");
                }

                // Get current user ID
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized("User not authenticated");
                }

                // Create uploads directory if not exists
                var uploadsPath = Path.Combine(_environment.ContentRootPath, "uploads", "attachments");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // Generate unique filename
                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsPath, uniqueFileName);

                // Save file to disk
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Create attachment entity
                var attachment = new Attachment
                {
                    FileName = file.FileName,
                    FilePath = uniqueFileName, // Store only filename, not full path
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    TaskId = taskId,
                    UploadedById = userId
                };

                var createdAttachment = await _attachmentRepository.CreateAsync(attachment);

                // Load navigation properties for DTO mapping
                var attachmentWithDetails = await _attachmentRepository.GetByIdAsync(createdAttachment.Id);

                var attachmentDto = _mapper.Map<AttachmentDto>(attachmentWithDetails);

                _logger.LogInformation($"File uploaded: {file.FileName} for Task {taskId} by User {userId}");

                await _notificationService.NotifyAttachmentAdded(attachmentWithDetails!);

                return CreatedAtAction(nameof(GetAttachment), new { id = attachmentDto.Id }, attachmentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading attachment");
                return StatusCode(500, "An error occurred while uploading the file");
            }
        }

        // GET: api/attachments/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<AttachmentDto>> GetAttachment(int id)
        {
            var attachment = await _attachmentRepository.GetByIdAsync(id);
            if (attachment == null)
            {
                return NotFound();
            }

            var attachmentDto = _mapper.Map<AttachmentDto>(attachment);
            return Ok(attachmentDto);
        }

        // GET: api/attachments/task/{taskId}
        [HttpGet("task/{taskId}")]
        public async Task<ActionResult<IEnumerable<AttachmentDto>>> GetTaskAttachments(int taskId)
        {
            var attachments = await _attachmentRepository.GetByTaskIdAsync(taskId);
            var attachmentDtos = _mapper.Map<IEnumerable<AttachmentDto>>(attachments);
            return Ok(attachmentDtos);
        }

        // GET: api/attachments/{id}/download
        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadAttachment(int id)
        {
            var attachment = await _attachmentRepository.GetByIdAsync(id);
            if (attachment == null)
            {
                return NotFound();
            }

            var filePath = Path.Combine(_environment.ContentRootPath, "uploads", "attachments", attachment.FilePath);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found on server");
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, attachment.ContentType, attachment.FileName);
        }

        // DELETE: api/attachments/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAttachment(int id)
        {
            var attachment = await _attachmentRepository.GetByIdAsync(id);
            if (attachment == null)
            {
                return NotFound();
            }

            // Check if user has permission (owner or admin)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (attachment.UploadedById != userId && userRole != "Admin")
            {
                return Forbid();
            }

            // Delete file from disk
            var filePath = Path.Combine(_environment.ContentRootPath, "uploads", "attachments", attachment.FilePath);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            await _attachmentRepository.DeleteAsync(id);

            await _notificationService.NotifyAttachmentDeleted(id, attachment.TaskId);

            _logger.LogInformation($"Attachment {id} deleted by User {userId}");

            return NoContent();
        }
    }
}