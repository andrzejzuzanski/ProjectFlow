import { api } from "./api";

export const TaskStatus = {
  ToDo: 0,
  InProgress: 1,
  Review: 2,
  Done: 3,
} as const;

export const TaskPriority = {
  Low: 0,
  Medium: 1,
  High: 2,
  Critical: 3,
} as const;

export type TaskStatus = (typeof TaskStatus)[keyof typeof TaskStatus];
export type TaskPriority = (typeof TaskPriority)[keyof typeof TaskPriority];

export interface UpdateTaskStatusRequest {
  status: TaskStatus;
}

export interface Task {
  id: number;
  title: string;
  description: string;
  status: TaskStatus;
  priority: TaskPriority;
  projectId: number;
  projectName: string;
  assignedToId?: number;
  assignedToName?: string;
  dueDate?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateTaskRequest {
  title: string;
  description: string;
  priority: TaskPriority;
  projectId: number;
  assignedToId?: number;
  dueDate?: string;
}

export interface UpdateTaskRequest {
  title: string;
  description: string;
  status: TaskStatus;
  priority: TaskPriority;
  assignedToId?: number;
  dueDate?: string;
}

export const taskService = {
  // Get tasks by project
  getByProject: async (projectId: number): Promise<Task[]> => {
    const response = await api.get(`/tasks/project/${projectId}`);
    return response.data;
  },

  // Get single task
  getById: async (id: number): Promise<Task> => {
    const response = await api.get(`/tasks/${id}`);
    return response.data;
  },

  // Get task with full details
  getWithDetails: async (id: number): Promise<Task> => {
    const response = await api.get(`/tasks/${id}/details`);
    return response.data;
  },

  // Create new task
  create: async (taskData: CreateTaskRequest): Promise<Task> => {
    const response = await api.post("/tasks", taskData);
    return response.data;
  },

  // Update task
  update: async (id: number, taskData: UpdateTaskRequest): Promise<Task> => {
    const response = await api.put(`/tasks/${id}`, taskData);
    return response.data;
  },

  // Delete task
  delete: async (id: number): Promise<void> => {
    await api.delete(`/tasks/${id}`);
  },

  updateStatus: async (id: number, status: TaskStatus): Promise<Task> => {
    const currentTask = await taskService.getById(id);

    const updateData: UpdateTaskRequest = {
      title: currentTask.title,
      description: currentTask.description,
      status: status,
      priority: currentTask.priority,
      assignedToId: currentTask.assignedToId,
      dueDate: currentTask.dueDate,
    };

    const response = await api.put(`/tasks/${id}`, updateData);
    return response.data;
  },
};

// Helper functions dla UI
export const getStatusName = (status: TaskStatus): string => {
  const statusNames = {
    [TaskStatus.ToDo]: "To Do",
    [TaskStatus.InProgress]: "In Progress",
    [TaskStatus.Review]: "Review",
    [TaskStatus.Done]: "Done",
  };
  return statusNames[status] || "Unknown";
};

export const getPriorityName = (priority: TaskPriority): string => {
  const priorityNames = {
    [TaskPriority.Low]: "Low",
    [TaskPriority.Medium]: "Medium",
    [TaskPriority.High]: "High",
    [TaskPriority.Critical]: "Critical",
  };
  return priorityNames[priority] || "Unknown";
};

export const getPriorityColor = (priority: TaskPriority): string => {
  const priorityColors = {
    [TaskPriority.Low]: "#28a745",
    [TaskPriority.Medium]: "#ffc107",
    [TaskPriority.High]: "#fd7e14",
    [TaskPriority.Critical]: "#dc3545",
  };
  return priorityColors[priority] || "#6c757d";
};
