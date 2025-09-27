import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { timeTrackingService } from "../services/timeTrackingService";
import { useTimer } from "../hooks/useTimer";
import { toastService } from "../services/toastService";

interface TaskTimerProps {
  taskId: number;
  taskTitle: string;
  hasActiveTimer?: boolean;
  activeTimerStart?: string;
  totalTimeMinutes?: number;
  style?: React.CSSProperties;
}

export default function TaskTimer({
  taskId,
  taskTitle,
  hasActiveTimer = false,
  activeTimerStart,
  totalTimeMinutes = 0,
  style,
}: TaskTimerProps) {
  const [description, setDescription] = useState("");
  const queryClient = useQueryClient();

  const { formattedTime } = useTimer(hasActiveTimer, activeTimerStart);

  // Get active timer
  const { data: activeTimer } = useQuery({
    queryKey: ["activeTimer"],
    queryFn: timeTrackingService.getActiveTimer,
    refetchInterval: 5000,
  });

  const startTimerMutation = useMutation({
    mutationFn: (data: { taskId: number; description?: string }) =>
      timeTrackingService.startTimer(data),
    onSuccess: () => {
      toastService.success(
        "Timer Started",
        `Started tracking time for "${taskTitle}"`
      );
      queryClient.invalidateQueries({ queryKey: ["tasks"] });
      queryClient.invalidateQueries({ queryKey: ["activeTimer"] });
    },
    onError: (error: unknown) => {
      let message = "Failed to start timer";

      if (error && typeof error === "object" && "response" in error) {
        const response = (error as { response?: { data?: string } }).response;
        message = response?.data || message;
      }

      toastService.error("Timer Error", message);
    },
  });

  const stopTimerMutation = useMutation({
    mutationFn: (timeEntryId: number) =>
      timeTrackingService.stopTimer(timeEntryId),
    onSuccess: (data) => {
      const minutes = Math.round(data.durationMinutes);
      toastService.success(
        "Timer Stopped",
        `Logged ${minutes} minutes for "${taskTitle}"`
      );
      queryClient.invalidateQueries({ queryKey: ["tasks"] });
      queryClient.invalidateQueries({ queryKey: ["activeTimer"] });
      setDescription("");
    },
  });

  const handleStartTimer = () => {
    startTimerMutation.mutate({
      taskId,
      description: description.trim() || undefined,
    });
  };

  const handleStopTimer = () => {
    if (activeTimer) {
      stopTimerMutation.mutate(activeTimer.id);
    }
  };

  const formatTotalTime = (minutes: number): string => {
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;
    if (hours > 0) {
      return `${hours}h ${mins}m`;
    }
    return `${mins}m`;
  };

  return (
    <div
      style={{
        border: "1px solid #e0e0e0",
        borderRadius: "6px",
        padding: "12px",
        backgroundColor: hasActiveTimer ? "#e8f5e8" : "#f8f9fa",
        marginTop: "8px",
        ...style,
      }}
    >
      <div
        style={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
        }}
      >
        <div>
          <div style={{ fontSize: "14px", fontWeight: "bold", color: "#333" }}>
            Time Tracking
          </div>
          <div style={{ fontSize: "12px", color: "#666", marginTop: "2px" }}>
            Total: {formatTotalTime(totalTimeMinutes)}
            {hasActiveTimer && (
              <span
                style={{
                  marginLeft: "10px",
                  color: "#28a745",
                  fontWeight: "bold",
                }}
              >
                Running: {formattedTime}
              </span>
            )}
          </div>
        </div>

        <div style={{ display: "flex", gap: "8px", alignItems: "center" }}>
          {!hasActiveTimer ? (
            <>
              <input
                type="text"
                placeholder="What are you working on?"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                style={{
                  padding: "6px 8px",
                  border: "1px solid #ddd",
                  borderRadius: "4px",
                  fontSize: "12px",
                  width: "150px",
                }}
              />
              <button
                onClick={handleStartTimer}
                disabled={startTimerMutation.isPending}
                style={{
                  padding: "6px 12px",
                  backgroundColor: "#28a745",
                  color: "white",
                  border: "none",
                  borderRadius: "4px",
                  cursor: "pointer",
                  fontSize: "12px",
                  fontWeight: "bold",
                }}
              >
                Start
              </button>
            </>
          ) : (
            <button
              onClick={handleStopTimer}
              disabled={stopTimerMutation.isPending}
              style={{
                padding: "6px 12px",
                backgroundColor: "#dc3545",
                color: "white",
                border: "none",
                borderRadius: "4px",
                cursor: "pointer",
                fontSize: "12px",
                fontWeight: "bold",
              }}
            >
              Stop
            </button>
          )}
        </div>
      </div>
    </div>
  );
}
