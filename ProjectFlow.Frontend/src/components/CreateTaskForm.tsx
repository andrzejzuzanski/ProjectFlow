import { useState } from "react";
import { useMutation, useQueryClient, useQuery } from "@tanstack/react-query";
import { taskService, TaskPriority } from "../services/taskService";
import type { CreateTaskRequest } from "../services/taskService";
import { userService } from "../services/userService";

interface CreateTaskFormProps {
  projectId: number;
  onSuccess: () => void;
  onCancel: () => void;
}

export default function CreateTaskForm({
  projectId,
  onSuccess,
  onCancel,
}: CreateTaskFormProps) {
  const queryClient = useQueryClient();
  const [formData, setFormData] = useState({
    title: "",
    description: "",
    priority: TaskPriority.Medium as TaskPriority,
    assignedToId: undefined as number | undefined,
    dueDate: "",
  });

  // Fetch users for assignment dropdown
  const { data: users } = useQuery({
    queryKey: ["users"],
    queryFn: userService.getAll,
  });

  const createMutation = useMutation({
    mutationFn: (taskData: CreateTaskRequest) => taskService.create(taskData),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["tasks", projectId.toString()],
      });
      onSuccess();
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    createMutation.mutate({
      ...formData,
      projectId,
      dueDate: formData.dueDate || undefined,
    });
  };

  return (
    <div
      style={{
        position: "fixed",
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        backgroundColor: "rgba(0,0,0,0.5)",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        zIndex: 1000,
        color: "black",
      }}
    >
      <div
        style={{
          backgroundColor: "white",
          padding: "30px",
          borderRadius: "8px",
          width: "500px",
          maxHeight: "80vh",
          overflow: "auto",
        }}
      >
        <h2>Create New Task</h2>
        <form onSubmit={handleSubmit}>
          <div style={{ marginBottom: "15px" }}>
            <label
              style={{
                display: "block",
                marginBottom: "5px",
                fontWeight: "bold",
              }}
            >
              Title *
            </label>
            <input
              type="text"
              value={formData.title}
              onChange={(e) =>
                setFormData({ ...formData, title: e.target.value })
              }
              required
              style={{
                width: "100%",
                padding: "8px",
                border: "1px solid #ccc",
                borderRadius: "4px",
              }}
            />
          </div>

          <div style={{ marginBottom: "15px" }}>
            <label
              style={{
                display: "block",
                marginBottom: "5px",
                fontWeight: "bold",
              }}
            >
              Description
            </label>
            <textarea
              value={formData.description}
              onChange={(e) =>
                setFormData({ ...formData, description: e.target.value })
              }
              rows={4}
              style={{
                width: "100%",
                padding: "8px",
                border: "1px solid #ccc",
                borderRadius: "4px",
                resize: "vertical",
              }}
            />
          </div>

          <div style={{ marginBottom: "15px" }}>
            <label
              style={{
                display: "block",
                marginBottom: "5px",
                fontWeight: "bold",
              }}
            >
              Priority
            </label>
            <select
              value={formData.priority}
              onChange={(e) =>
                setFormData({
                  ...formData,
                  priority: Number(e.target.value) as TaskPriority,
                })
              }
              style={{
                width: "100%",
                padding: "8px",
                border: "1px solid #ccc",
                borderRadius: "4px",
              }}
            >
              <option value={TaskPriority.Low}>Low</option>
              <option value={TaskPriority.Medium}>Medium</option>
              <option value={TaskPriority.High}>High</option>
              <option value={TaskPriority.Critical}>Critical</option>
            </select>
          </div>

          <div style={{ marginBottom: "15px" }}>
            <label
              style={{
                display: "block",
                marginBottom: "5px",
                fontWeight: "bold",
              }}
            >
              Assign To
            </label>
            <select
              value={formData.assignedToId || ""}
              onChange={(e) =>
                setFormData({
                  ...formData,
                  assignedToId: e.target.value
                    ? Number(e.target.value)
                    : undefined,
                })
              }
              style={{
                width: "100%",
                padding: "8px",
                border: "1px solid #ccc",
                borderRadius: "4px",
              }}
            >
              <option value="">Unassigned</option>
              {users?.map((user) => (
                <option key={user.id} value={user.id}>
                  {user.firstName} {user.lastName}
                </option>
              ))}
            </select>
          </div>

          <div style={{ marginBottom: "20px" }}>
            <label
              style={{
                display: "block",
                marginBottom: "5px",
                fontWeight: "bold",
              }}
            >
              Due Date
            </label>
            <input
              type="date"
              value={formData.dueDate}
              onChange={(e) =>
                setFormData({ ...formData, dueDate: e.target.value })
              }
              style={{
                width: "100%",
                padding: "8px",
                border: "1px solid #ccc",
                borderRadius: "4px",
              }}
            />
          </div>

          <div
            style={{ display: "flex", gap: "10px", justifyContent: "flex-end" }}
          >
            <button
              type="button"
              onClick={onCancel}
              style={{
                padding: "10px 20px",
                backgroundColor: "#6c757d",
                color: "white",
                border: "none",
                borderRadius: "4px",
                cursor: "pointer",
              }}
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={createMutation.isPending}
              style={{
                padding: "10px 20px",
                backgroundColor: createMutation.isPending ? "#ccc" : "#007bff",
                color: "white",
                border: "none",
                borderRadius: "4px",
                cursor: createMutation.isPending ? "not-allowed" : "pointer",
              }}
            >
              {createMutation.isPending ? "Creating..." : "Create Task"}
            </button>
          </div>
        </form>
        {createMutation.error && (
          <div style={{ color: "red", marginTop: "10px" }}>
            Error: {(createMutation.error as Error).message}
          </div>
        )}
      </div>
    </div>
  );
}
