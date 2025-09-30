import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useParams, Link } from "react-router-dom";
import {
  taskService,
  getPriorityName,
  getPriorityColor,
  TaskStatus,
  TaskPriority,
} from "../services/taskService";
import type { Task } from "../services/taskService";
import { useState } from "react";
import CreateTaskForm from "../components/CreateTaskForm";
import KanbanBoard from "../components/KanbanBoard";
import { signalRService } from "../services/signalRService";
import { useEffect } from "react";
import { toastService } from "../services/toastService";
import { userService } from "../services/userService";
import TaskTimer from "../components/TaskTimer";
import { timeTrackingService } from "../services/timeTrackingService";
import { AttachmentList } from "../components/AttachmentList";

export default function TasksPage() {
  const { projectId } = useParams<{ projectId: string }>();
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [viewMode, setViewMode] = useState<"list" | "kanban">("list");
  const [searchTerm, setSearchTerm] = useState("");
  const [filterStatus, setFilterStatus] = useState<TaskStatus | "all">("all");
  const [filterPriority, setFilterPriority] = useState<TaskPriority | "all">(
    "all"
  );
  const [filterAssignee, setFilterAssignee] = useState<number | "all">("all");
  const [showFilters, setShowFilters] = useState(false);
  const [expandedTaskId, setExpandedTaskId] = useState<number | null>(null);

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

  const { data: users } = useQuery({
    queryKey: ["users"],
    queryFn: userService.getAll,
  });

  const { data: activeTimer } = useQuery({
    queryKey: ["activeTimer"],
    queryFn: timeTrackingService.getActiveTimer,
    refetchInterval: 30000,
  });

  useEffect(() => {
    const setupSignalR = async () => {
      try {
        await signalRService.startConnection();

        if (projectId) {
          await signalRService.joinProject(Number(projectId));

          signalRService.offAllEvents();

          signalRService.onTaskCreated((taskNotification) => {
            if (taskNotification.projectId === Number(projectId)) {
              toastService.success(
                "Task Created",
                `"${taskNotification.title}" was added`
              );
              queryClient.invalidateQueries({ queryKey: ["tasks", projectId] });
            }
          });

          signalRService.onTimerStarted((timerNotification) => {
            if (timerNotification.taskId) {
              toastService.info(
                "Timer Started",
                `${timerNotification.userName} started timer on "${timerNotification.taskTitle}"`
              );
              queryClient.invalidateQueries({ queryKey: ["tasks", projectId] });
              queryClient.invalidateQueries({ queryKey: ["activeTimer"] });
            }
          });

          signalRService.onTimerStopped((timerNotification) => {
            if (timerNotification.taskId) {
              toastService.success(
                "Timer Stopped",
                `${timerNotification.userName} logged ${timerNotification.durationMinutes} minutes`
              );
              queryClient.invalidateQueries({ queryKey: ["tasks", projectId] });
              queryClient.invalidateQueries({ queryKey: ["activeTimer"] });
            }
          });

          signalRService.onTaskUpdated((taskNotification) => {
            if (taskNotification.projectId === Number(projectId)) {
              const statusNames = ["To Do", "In Progress", "Review", "Done"];
              toastService.info(
                "Task Updated",
                `"${taskNotification.title}" moved to ${
                  statusNames[taskNotification.status]
                }`
              );
              queryClient.invalidateQueries({ queryKey: ["tasks", projectId] });
            }
          });

          signalRService.onTaskDeleted((deleteNotification) => {
            if (deleteNotification.projectId === Number(projectId)) {
              toastService.warning(
                "Task Deleted",
                `Task #${deleteNotification.taskId} was removed`
              );
              queryClient.invalidateQueries({ queryKey: ["tasks", projectId] });
            }
          });

          signalRService.onAttachmentAdded((attachmentNotification) => {
            toastService.info(
              "Attachment Added",
              `📎 ${attachmentNotification.fileName} uploaded`
            );
            queryClient.invalidateQueries({
              queryKey: ["attachments", attachmentNotification.taskId],
            });
          });

          signalRService.onAttachmentDeleted((deleteNotification) => {
            toastService.warning(
              "Attachment Deleted",
              `File removed from task`
            );
            queryClient.invalidateQueries({
              queryKey: ["attachments", deleteNotification.taskId],
            });
          });
        }
      } catch (error) {
        console.error("SignalR setup failed:", error);
      }
    };

    setupSignalR();

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

  const getFilteredTasks = (tasks: Task[] | undefined) => {
    if (!tasks) return [];

    return tasks.filter((task) => {
      const matchesSearch =
        searchTerm === "" ||
        task.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
        task.description.toLowerCase().includes(searchTerm.toLowerCase());

      const matchesStatus =
        filterStatus === "all" || task.status === filterStatus;

      const matchesPriority =
        filterPriority === "all" || task.priority === filterPriority;

      const matchesAssignee =
        filterAssignee === "all" ||
        (filterAssignee === 0
          ? !task.assignedToId
          : task.assignedToId === filterAssignee);

      return (
        matchesSearch && matchesStatus && matchesPriority && matchesAssignee
      );
    });
  };

  const filteredTasks = getFilteredTasks(tasks);
  const activeFiltersCount = [
    searchTerm !== "",
    filterStatus !== "all",
    filterPriority !== "all",
    filterAssignee !== "all",
  ].filter(Boolean).length;

  if (isLoading) return <div style={{ padding: "20px" }}>Loading tasks...</div>;
  if (error)
    return (
      <div style={{ padding: "20px", color: "red" }}>Error loading tasks</div>
    );

  return (
    <>
      <style>{`
        @keyframes slideDown {
          from {
            opacity: 0;
            transform: translateY(-10px);
          }
          to {
            opacity: 1;
            transform: translateY(0);
          }
        }
      `}</style>

      <div style={{ padding: "20px" }}>
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
            <div style={{ position: "relative" }}>
              <input
                type="text"
                placeholder="Search tasks..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                style={{
                  padding: "8px 36px 8px 12px",
                  border: "1px solid #ddd",
                  borderRadius: "6px",
                  fontSize: "14px",
                  width: "200px",
                }}
              />
              <span
                style={{
                  position: "absolute",
                  right: "10px",
                  top: "50%",
                  transform: "translateY(-50%)",
                  color: "#999",
                }}
              >
                🔍
              </span>
            </div>

            <button
              onClick={() => setShowFilters(!showFilters)}
              style={{
                padding: "8px 16px",
                backgroundColor: showFilters ? "#007bff" : "#f8f9fa",
                color: showFilters ? "white" : "#333",
                border: "1px solid #ddd",
                borderRadius: "6px",
                cursor: "pointer",
                fontSize: "14px",
                position: "relative",
              }}
            >
              🔧 Filters
              {activeFiltersCount > 0 && (
                <span
                  style={{
                    position: "absolute",
                    top: "-8px",
                    right: "-8px",
                    backgroundColor: "#dc3545",
                    color: "white",
                    borderRadius: "50%",
                    width: "20px",
                    height: "20px",
                    fontSize: "12px",
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                  }}
                >
                  {activeFiltersCount}
                </span>
              )}
            </button>

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

        {showFilters && (
          <div
            style={{
              backgroundColor: "#f8f9fa",
              border: "1px solid #e0e0e0",
              borderRadius: "8px",
              padding: "20px",
              marginBottom: "20px",
              animation: "slideDown 0.3s ease-out",
            }}
          >
            <div
              style={{
                display: "grid",
                gridTemplateColumns: "repeat(auto-fit, minmax(200px, 1fr))",
                gap: "15px",
              }}
            >
              <div>
                <label
                  style={{
                    display: "block",
                    marginBottom: "5px",
                    fontWeight: "bold",
                    fontSize: "14px",
                  }}
                >
                  Status
                </label>
                <select
                  value={filterStatus}
                  onChange={(e) =>
                    setFilterStatus(
                      e.target.value === "all"
                        ? "all"
                        : (Number(e.target.value) as TaskStatus)
                    )
                  }
                  style={{
                    width: "100%",
                    padding: "8px",
                    border: "1px solid #ddd",
                    borderRadius: "4px",
                  }}
                >
                  <option value="all">All Statuses</option>
                  <option value={TaskStatus.ToDo}>To Do</option>
                  <option value={TaskStatus.InProgress}>In Progress</option>
                  <option value={TaskStatus.Review}>Review</option>
                  <option value={TaskStatus.Done}>Done</option>
                </select>
              </div>

              <div>
                <label
                  style={{
                    display: "block",
                    marginBottom: "5px",
                    fontWeight: "bold",
                    fontSize: "14px",
                  }}
                >
                  Priority
                </label>
                <select
                  value={filterPriority}
                  onChange={(e) =>
                    setFilterPriority(
                      e.target.value === "all"
                        ? "all"
                        : (Number(e.target.value) as TaskPriority)
                    )
                  }
                  style={{
                    width: "100%",
                    padding: "8px",
                    border: "1px solid #ddd",
                    borderRadius: "4px",
                  }}
                >
                  <option value="all">All Priorities</option>
                  <option value={TaskPriority.Low}>Low</option>
                  <option value={TaskPriority.Medium}>Medium</option>
                  <option value={TaskPriority.High}>High</option>
                  <option value={TaskPriority.Critical}>Critical</option>
                </select>
              </div>

              <div>
                <label
                  style={{
                    display: "block",
                    marginBottom: "5px",
                    fontWeight: "bold",
                    fontSize: "14px",
                  }}
                >
                  Assignee
                </label>
                <select
                  value={filterAssignee}
                  onChange={(e) =>
                    setFilterAssignee(
                      e.target.value === "all" ? "all" : Number(e.target.value)
                    )
                  }
                  style={{
                    width: "100%",
                    padding: "8px",
                    border: "1px solid #ddd",
                    borderRadius: "4px",
                  }}
                >
                  <option value="all">All Assignees</option>
                  <option value={0}>Unassigned</option>
                  {users?.map((user) => (
                    <option key={user.id} value={user.id}>
                      {user.firstName} {user.lastName}
                    </option>
                  ))}
                </select>
              </div>

              <div style={{ display: "flex", alignItems: "flex-end" }}>
                <button
                  onClick={() => {
                    setSearchTerm("");
                    setFilterStatus("all");
                    setFilterPriority("all");
                    setFilterAssignee("all");
                  }}
                  style={{
                    padding: "8px 16px",
                    backgroundColor: "#6c757d",
                    color: "white",
                    border: "none",
                    borderRadius: "4px",
                    cursor: "pointer",
                    fontSize: "14px",
                  }}
                >
                  Clear All
                </button>
              </div>
            </div>

            <div style={{ marginTop: "15px", fontSize: "14px", color: "#666" }}>
              Showing {filteredTasks.length} of {tasks?.length || 0} tasks
            </div>
          </div>
        )}

        {!filteredTasks || filteredTasks.length === 0 ? (
          <div
            style={{ textAlign: "center", padding: "40px", color: "#6c757d" }}
          >
            {tasks && tasks.length > 0 ? (
              <>
                <h3>No tasks match your filters</h3>
                <p>Try adjusting your search or filter criteria</p>
              </>
            ) : (
              <>
                <h3>No tasks yet</h3>
                <p>Create your first task to get started!</p>
              </>
            )}
          </div>
        ) : viewMode === "kanban" ? (
          <KanbanBoard
            tasks={filteredTasks}
            onTaskStatusChange={handleStatusChange}
          />
        ) : (
          <div style={{ display: "grid", gap: "15px" }}>
            {filteredTasks.map((task: Task) => (
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
                    cursor: "pointer",
                  }}
                  onClick={() =>
                    setExpandedTaskId(
                      expandedTaskId === task.id ? null : task.id
                    )
                  }
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
                        <option value={TaskStatus.InProgress}>
                          In Progress
                        </option>
                        <option value={TaskStatus.Review}>Review</option>
                        <option value={TaskStatus.Done}>Done</option>
                      </select>
                      <span>
                        Assigned: {task.assignedToName || "Unassigned"}
                      </span>
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
                {expandedTaskId === task.id && (
                  <div
                    style={{
                      marginTop: "15px",
                      paddingTop: "15px",
                      borderTop: "1px solid #e0e0e0",
                    }}
                  >
                    <AttachmentList taskId={task.id} />
                  </div>
                )}
                <TaskTimer
                  taskId={task.id}
                  taskTitle={task.title}
                  hasActiveTimer={activeTimer?.taskId === task.id}
                  activeTimerStart={
                    activeTimer?.taskId === task.id
                      ? activeTimer.startTime
                      : undefined
                  }
                  totalTimeMinutes={task.totalTimeMinutes || 0}
                />
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
    </>
  );
}
