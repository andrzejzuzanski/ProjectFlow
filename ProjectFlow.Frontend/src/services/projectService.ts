import { api } from "./api";

export interface Project {
  id: number;
  name: string;
  description: string;
  status: number;
  startDate?: string;
  endDate?: string;
  createdById: number;
  createdByName: string;
  createdAt: string;
  taskCount: number;
  completedTaskCount: number;
}

export interface CreateProjectRequest {
  name: string;
  description: string;
  startDate?: string;
  endDate?: string;
  createdById: number;
}

export const projectService = {
  // Get all projects
  getAll: async (): Promise<Project[]> => {
    console.log("1. Starting API call...");
    try {
      const response = await api.get("/projects");
      console.log("2. API response:", response.data);
      console.log("3. Returning data...");
      return response.data;
    } catch (error) {
      console.error("4. API error:", error);
      throw error;
    }
  },

  // Get a project by ID
  getById: async (id: number): Promise<Project> => {
    const response = await api.get(`/projects/${id}`);
    return response.data;
  },

  // Create a new project
  create: async (projectData: CreateProjectRequest): Promise<Project> => {
    const response = await api.post("/projects", projectData);
    return response.data;
  },
};
