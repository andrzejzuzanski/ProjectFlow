import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useParams, Link } from "react-router-dom";
import {
  taskService,
  getPriorityName,
  getPriorityColor,
  TaskStatus,
} from "../services/taskService";
import type { Task } from "../services/taskService";
import { useState } from "react";
import CreateTaskForm from "../components/CreateTaskForm";
import KanbanBoard from "../components/KanbanBoard";
import { signalRService } from "../services/signalRService";
import { useEffect } from "react";

export default function TasksPage() {
  const { projectId } = useParams<{ projectId: string }>();
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [viewMode, setViewMode] = useState<"list" | "kanban">("list");

  const queryClient = useQueryClient();
  const {
    data: tasks,
    isLoading,
    error,
  } = useQuery({
    queryKey: ["tasks", projectId],
    queryFn: () => taskService.getByProject(Number(projectId)),
    enabled: !!projectId,
  });

  useEffect(() => {
    const setupSignalR = async () => {
      try {
        await signalRService.startConnection();

        if (projectId) {
          await signalRService.joinProject(Number(projectId));

          // Live task created
          signalRService.onTaskCreated((taskNotification) => {
            console.log("Task created:", taskNotification);
            queryClient.invalidateQueries({ queryKey: ["tasks", projectId] });
          });

          // Live task updated
          signalRService.onTaskUpdated((taskNotification) => {
            console.log("Task updated:", taskNotification);
            queryClient.invalidateQueries({ queryKey: ["tasks", projectId] });
          });

          // Live task deleted
          signalRService.onTaskDeleted((deleteNotification) => {
            console.log("Task deleted:", deleteNotification);
            queryClient.invalidateQueries({ queryKey: ["tasks", projectId] });
          });
        }
      } catch (error) {
        console.error("SignalR setup failed:", error);
      }
    };

    setupSignalR();

    // Cleanup
    return () => {
      signalRService.leaveProject();
    };
  }, [projectId, queryClient]);

  const updateTaskMutation = useMutation({
    mutationFn: ({ taskId, status }: { taskId: number; status: TaskStatus }) =>
      taskService.updateStatus(taskId, status),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["tasks", projectId] });
    },
  });

  const handleStatusChange = (taskId: number, newStatus: TaskStatus) => {
    updateTaskMutation.mutate({ taskId, status: newStatus });
  };

  if (isLoading) return <div style={{ padding: "20px" }}>Loading tasks...</div>;
  if (error)
    return (
      <div style={{ padding: "20px", color: "red" }}>Error loading tasks</div>
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
        <div
          style={{
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
            marginBottom: "20px",
          }}
        >
          <div>
            <Link
              to="/projects"
              style={{ color: "#007bff", textDecoration: "none" }}
            >
              ← Back to Projects
            </Link>
            <h1>Tasks - Project #{projectId}</h1>
          </div>

          <div style={{ display: "flex", gap: "10px", alignItems: "center" }}>
            {/* View Mode Toggle */}
            <div
              style={{
                display: "flex",
                backgroundColor: "#f8f9fa",
                borderRadius: "6px",
                padding: "2px",
              }}
            >
              <button
                onClick={() => setViewMode("list")}
                style={{
                  padding: "8px 16px",
                  backgroundColor:
                    viewMode === "list" ? "#007bff" : "transparent",
                  color: viewMode === "list" ? "white" : "#007bff",
                  border: "none",
                  borderRadius: "4px",
                  cursor: "pointer",
                  fontSize: "14px",
                }}
              >
                📋 List
              </button>
              <button
                onClick={() => setViewMode("kanban")}
                style={{
                  padding: "8px 16px",
                  backgroundColor:
                    viewMode === "kanban" ? "#007bff" : "transparent",
                  color: viewMode === "kanban" ? "white" : "#007bff",
                  border: "none",
                  borderRadius: "4px",
                  cursor: "pointer",
                  fontSize: "14px",
                }}
              >
                📊 Kanban
              </button>
            </div>

            <button
              onClick={() => setShowCreateForm(true)}
              style={{
                padding: "10px 20px",
                backgroundColor: "#28a745",
                color: "white",
                border: "none",
                borderRadius: "4px",
                cursor: "pointer",
              }}
            >
              + New Task
            </button>
          </div>
        </div>
      </div>

      {!tasks || tasks.length === 0 ? (
        <div style={{ textAlign: "center", padding: "40px", color: "#6c757d" }}>
          <h3>No tasks yet</h3>
          <p>Create your first task to get started!</p>
        </div>
      ) : viewMode === "kanban" ? (
        <KanbanBoard tasks={tasks} onTaskStatusChange={handleStatusChange} />
      ) : (
        <div style={{ display: "grid", gap: "15px" }}>
          {tasks.map((task: Task) => (
            <div
              key={task.id}
              style={{
                border: "1px solid #e0e0e0",
                borderRadius: "8px",
                padding: "15px",
                backgroundColor: "#fff",
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
                  <h4 style={{ margin: "0 0 8px 0", color: "#333" }}>
                    {task.title}
                  </h4>
                  <p
                    style={{
                      margin: "0 0 10px 0",
                      color: "#666",
                      fontSize: "14px",
                    }}
                  >
                    {task.description}
                  </p>
                  <div
                    style={{
                      display: "flex",
                      gap: "15px",
                      fontSize: "13px",
                      color: "#888",
                    }}
                  >
                    <select
                      value={task.status}
                      onChange={(e) =>
                        handleStatusChange(
                          task.id,
                          Number(e.target.value) as TaskStatus
                        )
                      }
                      style={{
                        padding: "2px 8px",
                        border: "1px solid #ccc",
                        borderRadius: "3px",
                        fontSize: "12px",
                      }}
                    >
                      <option value={TaskStatus.ToDo}>To Do</option>
                      <option value={TaskStatus.InProgress}>In Progress</option>
                      <option value={TaskStatus.Review}>Review</option>
                      <option value={TaskStatus.Done}>Done</option>
                    </select>
                    <span>Assigned: {task.assignedToName || "Unassigned"}</span>
                    {task.dueDate && (
                      <span>
                        Due: {new Date(task.dueDate).toLocaleDateString()}
                      </span>
                    )}
                  </div>
                </div>
                <div style={{ marginLeft: "15px" }}>
                  <span
                    style={{
                      padding: "4px 8px",
                      borderRadius: "12px",
                      fontSize: "11px",
                      fontWeight: "bold",
                      color: "white",
                      backgroundColor: getPriorityColor(task.priority),
                    }}
                  >
                    {getPriorityName(task.priority)}
                  </span>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
      {showCreateForm && (
        <CreateTaskForm
          projectId={Number(projectId)}
          onSuccess={() => setShowCreateForm(false)}
          onCancel={() => setShowCreateForm(false)}
        />
      )}
    </div>
  );
}
