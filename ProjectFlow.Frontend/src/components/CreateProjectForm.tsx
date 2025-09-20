import { useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useAuth } from "../contexts/AuthContext";
import { projectService } from "../services/projectService";
import type { CreateProjectRequest } from "../services/projectService";

interface Props {
  onSuccess?: () => void;
  onCancel?: () => void;
}

export default function CreateProjectForm({ onSuccess, onCancel }: Props) {
  const { user } = useAuth();
  const queryClient = useQueryClient();

  const [formData, setFormData] = useState<CreateProjectRequest>({
    name: "",
    description: "",
    startDate: "",
    endDate: "",
    createdById: user?.id || 0,
  });

  const createProjectMutation = useMutation({
    mutationFn: projectService.create,
    onSuccess: (newProject) => {
      console.log("Project created:", newProject);
      // Refresh projects list
      queryClient.invalidateQueries({ queryKey: ["projects"] });
      onSuccess?.();
    },
    onError: (error) => {
      console.error("Create project error:", error);
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    createProjectMutation.mutate(formData);
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
        <h2 style={{ marginTop: 0 }}>Create New Project</h2>

        <form onSubmit={handleSubmit}>
          <div style={{ marginBottom: "15px" }}>
            <label
              style={{
                display: "block",
                marginBottom: "5px",
                fontWeight: "bold",
              }}
            >
              Project Name *
            </label>
            <input
              type="text"
              value={formData.name}
              onChange={(e) =>
                setFormData({ ...formData, name: e.target.value })
              }
              required
              style={{
                width: "100%",
                padding: "8px",
                border: "1px solid #ccc",
                borderRadius: "4px",
                fontSize: "14px",
              }}
              placeholder="Enter project name"
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
                fontSize: "14px",
                resize: "vertical",
              }}
              placeholder="Describe your project"
            />
          </div>

          <div style={{ display: "flex", gap: "15px", marginBottom: "20px" }}>
            <div style={{ flex: 1 }}>
              <label
                style={{
                  display: "block",
                  marginBottom: "5px",
                  fontWeight: "bold",
                }}
              >
                Start Date
              </label>
              <input
                type="date"
                value={formData.startDate}
                onChange={(e) =>
                  setFormData({ ...formData, startDate: e.target.value })
                }
                style={{
                  width: "100%",
                  padding: "8px",
                  border: "1px solid #ccc",
                  borderRadius: "4px",
                  fontSize: "14px",
                }}
              />
            </div>

            <div style={{ flex: 1 }}>
              <label
                style={{
                  display: "block",
                  marginBottom: "5px",
                  fontWeight: "bold",
                }}
              >
                End Date
              </label>
              <input
                type="date"
                value={formData.endDate}
                onChange={(e) =>
                  setFormData({ ...formData, endDate: e.target.value })
                }
                style={{
                  width: "100%",
                  padding: "8px",
                  border: "1px solid #ccc",
                  borderRadius: "4px",
                  fontSize: "14px",
                }}
              />
            </div>
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
              disabled={
                createProjectMutation.isPending || !formData.name.trim()
              }
              style={{
                padding: "10px 20px",
                backgroundColor: createProjectMutation.isPending
                  ? "#ccc"
                  : "#007bff",
                color: "white",
                border: "none",
                borderRadius: "4px",
                cursor: createProjectMutation.isPending
                  ? "not-allowed"
                  : "pointer",
              }}
            >
              {createProjectMutation.isPending
                ? "Creating..."
                : "Create Project"}
            </button>
          </div>
        </form>

        {createProjectMutation.error && (
          <div style={{ color: "red", marginTop: "15px", fontSize: "14px" }}>
            Error: {(createProjectMutation.error as Error).message}
          </div>
        )}
      </div>
    </div>
  );
}
