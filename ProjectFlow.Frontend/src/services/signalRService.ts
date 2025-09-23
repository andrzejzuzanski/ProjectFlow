import {
  HubConnection,
  HubConnectionBuilder,
  LogLevel,
} from "@microsoft/signalr";

interface TaskNotification {
  taskId: number;
  title: string;
  status: number;
  priority: number;
  projectId: number;
  assignedToId?: number;
  createdAt?: string;
  updatedAt?: string;
}

interface TaskDeletedNotification {
  taskId: number;
  projectId: number;
}

class SignalRService {
  private connection: HubConnection | null = null;
  private currentProjectId: number | null = null;

  async startConnection(): Promise<void> {
    try {
      this.connection = new HubConnectionBuilder()
        .withUrl("https://localhost:7132/hubs/task-updates", {
          withCredentials: true,
        })
        .withAutomaticReconnect()
        .configureLogging(LogLevel.Information)
        .build();

      await this.connection.start();
      console.log("SignalR Connected");
    } catch (error) {
      console.error("SignalR Connection failed:", error);
    }
  }

  async joinProject(projectId: number): Promise<void> {
    if (this.connection && this.connection.state === "Connected") {
      await this.connection.invoke("JoinProjectGroup", projectId.toString());
      this.currentProjectId = projectId;
      console.log(`Joined project ${projectId} group`);
    }
  }

  async leaveProject(): Promise<void> {
    if (
      this.connection &&
      this.currentProjectId &&
      this.connection.state === "Connected"
    ) {
      try {
        await this.connection.invoke(
          "LeaveProjectGroup",
          this.currentProjectId.toString()
        );
      } catch (error) {
        console.warn("Failed to leave project group:", error);
      }
      this.currentProjectId = null;
    }
  }

  onTaskCreated(callback: (task: TaskNotification) => void): void {
    this.connection?.on("TaskCreated", callback);
  }

  onTaskUpdated(callback: (task: TaskNotification) => void): void {
    this.connection?.on("TaskUpdated", callback);
  }

  onTaskDeleted(callback: (data: TaskDeletedNotification) => void): void {
    this.connection?.on("TaskDeleted", callback);
  }

  offAllEvents(): void {
    this.connection?.off("TaskCreated");
    this.connection?.off("TaskUpdated");
    this.connection?.off("TaskDeleted");
  }

  async stopConnection(): Promise<void> {
    if (this.connection) {
      await this.leaveProject();
      try {
        await this.connection.stop();
      } catch (error) {
        console.warn("SignalR disconnect error:", error);
      }
      console.log("SignalR Disconnected");
    }
  }
}

export const signalRService = new SignalRService();
