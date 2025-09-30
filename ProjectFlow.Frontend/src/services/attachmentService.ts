import { api } from "./api";

export interface Attachment {
  id: number;
  fileName: string;
  contentType: string;
  fileSize: number;
  taskId: number;
  uploadedById: number;
  uploadedByName: string;
  createdAt: string;
}

export interface UploadAttachmentResponse {
  id: number;
  fileName: string;
  contentType: string;
  fileSize: number;
  taskId: number;
  uploadedById: number;
  uploadedByName: string;
  createdAt: string;
}

export const attachmentService = {
  // Upload file to task
  upload: async (
    taskId: number,
    file: File
  ): Promise<UploadAttachmentResponse> => {
    const formData = new FormData();
    formData.append("file", file);

    const response = await api.post(`/attachments/upload/${taskId}`, formData, {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    });
    return response.data;
  },

  // Get all attachments for a task
  getByTaskId: async (taskId: number): Promise<Attachment[]> => {
    const response = await api.get(`/attachments/task/${taskId}`);
    return response.data;
  },

  // Download attachment
  download: async (attachmentId: number, fileName: string): Promise<void> => {
    const response = await api.get(`/attachments/${attachmentId}/download`, {
      responseType: "blob",
    });

    // Create download link
    const url = window.URL.createObjectURL(new Blob([response.data]));
    const link = document.createElement("a");
    link.href = url;
    link.setAttribute("download", fileName);
    document.body.appendChild(link);
    link.click();
    link.remove();
    window.URL.revokeObjectURL(url);
  },

  // Delete attachment
  delete: async (attachmentId: number): Promise<void> => {
    await api.delete(`/attachments/${attachmentId}`);
  },

  // Format file size for display
  formatFileSize: (bytes: number): string => {
    if (bytes === 0) return "0 Bytes";
    const k = 1024;
    const sizes = ["Bytes", "KB", "MB", "GB"];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + " " + sizes[i];
  },

  // Get file icon based on content type
  getFileIcon: (contentType: string): string => {
    if (contentType.startsWith("image/")) return "🖼️";
    if (contentType.includes("pdf")) return "📄";
    if (contentType.includes("word") || contentType.includes("document"))
      return "📝";
    if (contentType.includes("excel") || contentType.includes("spreadsheet"))
      return "📊";
    if (contentType.includes("zip") || contentType.includes("compressed"))
      return "📦";
    return "📎";
  },
};
