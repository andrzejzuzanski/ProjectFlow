import { useQuery } from "@tanstack/react-query";
import { projectService } from "../services/projectService";
import type { Project } from "../services/projectService";
import { useState } from "react";
import CreateProjectForm from "../components/CreateProjectForm";
import { Link } from "react-router-dom";

export default function ProjectsPage() {
  const [showCreateForm, setShowCreateForm] = useState(false);

  const {
    data: projects,
    isLoading,
    error,
  } = useQuery({
    queryKey: ["projects"],
    queryFn: projectService.getAll,
  });

  const getStatusName = (status: number) => {
    const statuses = ["Planning", "Active", "On Hold", "Completed"];
    return statuses[status] || "Unknown";
  };

  const getStatusColor = (status: number) => {
    const colors = ["#6c757d", "#28a745", "#ffc107", "#17a2b8"];
    return colors[status] || "#6c757d";
  };

  if (isLoading)
    return <div style={{ padding: "20px" }}>Loading projects...</div>;
  if (error)
    return (
      <div style={{ padding: "20px", color: "red" }}>
        Error loading projects
      </div>
    );

  return (
    <div style={{ padding: "20px" }}>
      <div
        style={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          marginBottom: "20px",
        }}
      >
        <h1>Projects</h1>
        <button
          onClick={() => setShowCreateForm(true)}
          style={{
            padding: "10px 20px",
            backgroundColor: "#007bff",
            color: "white",
            border: "none",
            borderRadius: "4px",
            cursor: "pointer",
          }}
        >
          + New Project
        </button>
      </div>

      {!projects || projects.length === 0 ? (
        <div style={{ textAlign: "center", padding: "40px", color: "#6c757d" }}>
          <h3>No projects yet</h3>
          <p>Create your first project to get started!</p>
        </div>
      ) : (
        <div style={{ display: "grid", gap: "20px" }}>
          {projects.map((project: Project) => (
            <div
              key={project.id}
              style={{
                border: "1px solid #e0e0e0",
                borderRadius: "8px",
                padding: "20px",
                backgroundColor: "#fff",
                boxShadow: "0 2px 4px rgba(0,0,0,0.1)",
              }}
            >
              <div
                style={{
                  display: "flex",
                  justifyContent: "space-between",
                  alignItems: "flex-start",
                }}
              >
                <div style={{ flex: 1 }}>
                  <h3 style={{ margin: "0 0 10px 0", color: "#333" }}>
                    {project.name}
                  </h3>
                  <p style={{ margin: "0 0 15px 0", color: "#666" }}>
                    {project.description}
                  </p>
                  <div
                    style={{
                      display: "flex",
                      gap: "20px",
                      fontSize: "14px",
                      color: "#888",
                    }}
                  >
                    <span>Created by: {project.createdByName}</span>
                    <span>
                      Tasks: {project.completedTaskCount}/{project.taskCount}
                    </span>
                    <span>
                      Created:{" "}
                      {new Date(project.createdAt).toLocaleDateString()}
                    </span>
                  </div>
                </div>
                <div style={{ marginLeft: "20px" }}>
                  <span
                    style={{
                      padding: "4px 12px",
                      borderRadius: "12px",
                      fontSize: "12px",
                      fontWeight: "bold",
                      color: "white",
                      backgroundColor: getStatusColor(project.status),
                    }}
                  >
                    {getStatusName(project.status)}
                  </span>
                </div>
              </div>
              <div
                style={{
                  marginTop: "15px",
                  paddingTop: "15px",
                  borderTop: "1px solid #f0f0f0",
                }}
              >
                <Link
                  to={`/projects/${project.id}/tasks`}
                  style={{
                    padding: "8px 16px",
                    backgroundColor: "#28a745",
                    color: "white",
                    textDecoration: "none",
                    borderRadius: "4px",
                    fontSize: "14px",
                  }}
                >
                  View Tasks ({project.taskCount})
                </Link>
              </div>
            </div>
          ))}
        </div>
      )}
      {showCreateForm && (
        <CreateProjectForm
          onSuccess={() => setShowCreateForm(false)}
          onCancel={() => setShowCreateForm(false)}
        />
      )}
    </div>
  );
}
