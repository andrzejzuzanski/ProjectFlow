using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectFlow.Core.Entities
{
    public class Attachment
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public int TaskId { get; set; }
        public int UploadedById { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public ProjectTask Task { get; set; } = null!;
        public User UploadedBy { get; set; } = null!;
    }
}
