import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { attachmentService } from "../services/attachmentService";
import type { Attachment } from "../services/attachmentService";
import { toastService } from "../services/toastService";
import "./AttachmentList.css";

interface AttachmentListProps {
  taskId: number;
}

interface ApiError {
  response?: {
    data?: string;
  };
  message: string;
}

export function AttachmentList({ taskId }: AttachmentListProps) {
  const [uploading, setUploading] = useState(false);
  const queryClient = useQueryClient();

  // Fetch attachments for task
  const { data: attachments, isLoading } = useQuery<Attachment[]>({
    queryKey: ["attachments", taskId],
    queryFn: () => attachmentService.getByTaskId(taskId),
  });

  // Upload mutation
  const uploadMutation = useMutation({
    mutationFn: (file: File) => attachmentService.upload(taskId, file),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["attachments", taskId] });
      toastService.success("File Uploaded", "Attachment added successfully");
      setUploading(false);
    },
    onError: (error: ApiError) => {
      toastService.error(
        "Upload Failed",
        error.response?.data || error.message || "Failed to upload file"
      );
      setUploading(false);
    },
  });

  // Delete mutation
  const deleteMutation = useMutation({
    mutationFn: (attachmentId: number) =>
      attachmentService.delete(attachmentId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["attachments", taskId] });
      toastService.success("Deleted", "Attachment removed");
    },
    onError: () => {
      toastService.error("Delete Failed", "Could not delete attachment");
    },
  });

  const handleFileSelect = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file) {
      // Validate file size (10MB)
      if (file.size > 10 * 1024 * 1024) {
        toastService.error("File Too Large", "Maximum file size is 10MB");
        return;
      }

      setUploading(true);
      uploadMutation.mutate(file);
      event.target.value = "";
    }
  };

  const handleDownload = (attachment: Attachment) => {
    attachmentService.download(attachment.id, attachment.fileName);
    toastService.info("Downloading", attachment.fileName);
  };

  const handleDelete = (attachment: Attachment) => {
    if (
      window.confirm(`Delete "${attachment.fileName}"? This cannot be undone.`)
    ) {
      deleteMutation.mutate(attachment.id);
    }
  };

  if (isLoading) {
    return (
      <div className="attachment-list-loading">Loading attachments...</div>
    );
  }

  return (
    <div className="attachment-list">
      <div className="attachment-list-header">
        <h3>
          📎 Attachments{" "}
          {attachments && attachments.length > 0 && (
            <span className="attachment-count">({attachments.length})</span>
          )}
        </h3>
        <label className="upload-button">
          {uploading ? "Uploading..." : "+ Upload File"}
          <input
            type="file"
            onChange={handleFileSelect}
            disabled={uploading}
            style={{ display: "none" }}
          />
        </label>
      </div>

      {attachments && attachments.length > 0 ? (
        <div className="attachments-grid">
          {attachments.map((attachment) => (
            <div key={attachment.id} className="attachment-card">
              <div className="attachment-icon">
                {attachmentService.getFileIcon(attachment.contentType)}
              </div>
              <div className="attachment-info">
                <div className="attachment-name" title={attachment.fileName}>
                  {attachment.fileName}
                </div>
                <div className="attachment-meta">
                  {attachmentService.formatFileSize(attachment.fileSize)} •{" "}
                  {attachment.uploadedByName}
                </div>
                <div className="attachment-date">
                  {new Date(attachment.createdAt).toLocaleDateString()}
                </div>
              </div>
              <div className="attachment-actions">
                <button
                  className="attachment-action-btn download"
                  onClick={() => handleDownload(attachment)}
                  title="Download"
                >
                  ⬇️
                </button>
                <button
                  className="attachment-action-btn delete"
                  onClick={() => handleDelete(attachment)}
                  title="Delete"
                  disabled={deleteMutation.isPending}
                >
                  🗑️
                </button>
              </div>
            </div>
          ))}
        </div>
      ) : (
        <div className="no-attachments">
          <p>No attachments yet</p>
          <p className="hint">Upload files to share with your team</p>
        </div>
      )}
    </div>
  );
}
