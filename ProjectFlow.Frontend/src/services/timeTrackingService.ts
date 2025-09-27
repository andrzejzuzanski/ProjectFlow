import { api } from "./api";

export interface TimeEntry {
  id: number;
  taskId: number;
  userId: number;
  userName: string;
  startTime: string;
  endTime?: string;
  durationMinutes: number;
  description: string;
  createdAt: string;
}

export interface CreateTimeEntryRequest {
  taskId: number;
  description?: string;
}

export const timeTrackingService = {
  // Start timer
  startTimer: async (request: CreateTimeEntryRequest): Promise<TimeEntry> => {
    const response = await api.post("/time-entries", request);
    return response.data;
  },

  // Stop timer
  stopTimer: async (timeEntryId: number): Promise<TimeEntry> => {
    const response = await api.put(`/time-entries/${timeEntryId}/stop`);
    return response.data;
  },

  // Get time entries for task
  getByTask: async (taskId: number): Promise<TimeEntry[]> => {
    const response = await api.get(`/time-entries/task/${taskId}`);
    return response.data;
  },

  // Get active timer for user
  getActiveTimer: async (): Promise<TimeEntry | null> => {
    const response = await api.get("/time-entries/active");
    return response.data;
  },

  // Update time entry
  update: async (
    id: number,
    data: { description: string }
  ): Promise<TimeEntry> => {
    const response = await api.put(`/time-entries/${id}`, data);
    return response.data;
  },

  // Delete time entry
  delete: async (id: number): Promise<void> => {
    await api.delete(`/time-entries/${id}`);
  },
};
