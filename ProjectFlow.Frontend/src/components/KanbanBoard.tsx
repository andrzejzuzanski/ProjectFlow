import {
  DndContext,
  DragOverlay,
  PointerSensor,
  useSensor,
  useSensors,
  useDroppable,
} from "@dnd-kit/core";
import {
  SortableContext,
  useSortable,
  verticalListSortingStrategy,
} from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import { useState } from "react";
import {
  TaskStatus,
  getPriorityColor,
  getPriorityName,
} from "../services/taskService";
import type { Task } from "../services/taskService";
import type { DragEndEvent, DragStartEvent } from "@dnd-kit/core";
import TaskTimer from "./TaskTimer";
import { timeTrackingService } from "../services/timeTrackingService";
import { useQuery } from "@tanstack/react-query";

interface KanbanBoardProps {
  tasks: Task[];
  onTaskStatusChange: (taskId: number, newStatus: TaskStatus) => void;
}

interface ActiveTimer {
  taskId: number;
  startTime: string;
  id: number;
  userId: number;
  userName: string;
  endTime?: string;
  durationMinutes: number;
  description: string;
  createdAt: string;
}

const columns = [
  { id: TaskStatus.ToDo, title: "To Do", color: "#6c757d" },
  { id: TaskStatus.InProgress, title: "In Progress", color: "#007bff" },
  { id: TaskStatus.Review, title: "Review", color: "#ffc107" },
  { id: TaskStatus.Done, title: "Done", color: "#28a745" },
];

function DroppableColumn({
  children,
  id,
  title,
  color,
  taskCount,
}: {
  children: React.ReactNode;
  id: string;
  title: string;
  color: string;
  taskCount: number;
}) {
  const { setNodeRef, isOver } = useDroppable({ id });

  return (
    <div
      style={{
        backgroundColor: "#f8f9fa",
        borderRadius: "8px",
        padding: "15px",
      }}
    >
      <h3 style={{ margin: "0 0 15px 0", color: color, textAlign: "center" }}>
        {title} ({taskCount})
      </h3>

      <div
        ref={setNodeRef}
        style={{
          minHeight: "500px",
          backgroundColor: isOver ? "#e9ecef" : "transparent",
          borderRadius: "4px",
          transition: "background-color 0.2s ease",
          border: isOver ? "2px dashed #007bff" : "2px dashed transparent",
        }}
      >
        {children}
      </div>
    </div>
  );
}

function TaskCard({
  task,
  activeTimer,
}: {
  task: Task;
  activeTimer?: ActiveTimer | null;
}) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({
    id: task.id,
  });

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
  };

  return (
    <div ref={setNodeRef} style={style} {...attributes} {...listeners}>
      <div
        style={{
          backgroundColor: "white",
          border: "1px solid #e0e0e0",
          borderRadius: "6px",
          padding: "12px",
          marginBottom: "10px",
          boxShadow: isDragging
            ? "0 4px 8px rgba(0,0,0,0.2)"
            : "0 2px 4px rgba(0,0,0,0.1)",
          cursor: "grab",
          opacity: isDragging ? 0.5 : 1,
        }}
      >
        <h4 style={{ margin: "0 0 8px 0", fontSize: "14px", color: "#333" }}>
          {task.title}
        </h4>
        <p
          style={{
            margin: "0 0 10px 0",
            fontSize: "12px",
            color: "#666",
            lineHeight: "1.4",
          }}
        >
          {task.description.length > 80
            ? `${task.description.substring(0, 80)}...`
            : task.description}
        </p>

        <TaskTimer
          taskId={task.id}
          taskTitle={task.title}
          hasActiveTimer={activeTimer?.taskId === task.id}
          activeTimerStart={
            activeTimer?.taskId === task.id ? activeTimer.startTime : undefined
          }
          totalTimeMinutes={task.totalTimeMinutes || 0}
        />

        <div
          style={{
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
          }}
        >
          <span style={{ fontSize: "11px", color: "#888" }}>
            {task.assignedToName || "Unassigned"}
          </span>
          <span
            style={{
              padding: "2px 6px",
              borderRadius: "8px",
              fontSize: "10px",
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
  );
}

export default function KanbanBoard({
  tasks,
  onTaskStatusChange,
}: KanbanBoardProps) {
  const { data: activeTimer } = useQuery({
    queryKey: ["activeTimer"],
    queryFn: timeTrackingService.getActiveTimer,
    refetchInterval: 30000,
  });

  const [activeTask, setActiveTask] = useState<Task | null>(null);

  const sensors = useSensors(useSensor(PointerSensor));

  const getTasksByStatus = (status: TaskStatus) => {
    return tasks.filter((task) => task.status === status);
  };

  const handleDragStart = (event: DragStartEvent) => {
    const task = tasks.find((t) => t.id === Number(event.active.id));
    setActiveTask(task || null);
  };

  const handleDragEnd = (event: DragEndEvent) => {
    const { active, over } = event;

    if (!over) {
      setActiveTask(null);
      return;
    }

    const taskId = Number(active.id);
    const overId = Number(over.id);

    const validStatuses = [
      TaskStatus.ToDo,
      TaskStatus.InProgress,
      TaskStatus.Review,
      TaskStatus.Done,
    ];

    if (!validStatuses.includes(overId as TaskStatus)) {
      console.log("Invalid drop target, ignoring:", overId);
      setActiveTask(null);
      return;
    }

    const newStatus = overId as TaskStatus;
    const currentTask = tasks.find((t) => t.id === taskId);

    if (currentTask && currentTask.status === newStatus) {
      console.log("Same status, ignoring move");
      setActiveTask(null);
      return;
    }

    console.log("Valid status change:", { taskId, newStatus });
    onTaskStatusChange(taskId, newStatus);
    setActiveTask(null);
  };

  return (
    <DndContext
      sensors={sensors}
      onDragStart={handleDragStart}
      onDragEnd={handleDragEnd}
    >
      <div
        style={{
          display: "grid",
          gridTemplateColumns: "repeat(4, 1fr)",
          gap: "20px",
          padding: "20px",
        }}
      >
        {columns.map((column) => {
          const columnTasks = getTasksByStatus(column.id);
          return (
            <DroppableColumn
              key={column.id}
              id={column.id.toString()}
              title={column.title}
              color={column.color}
              taskCount={columnTasks.length}
            >
              <SortableContext
                items={columnTasks.map((t) => t.id)}
                strategy={verticalListSortingStrategy}
              >
                {columnTasks.map((task) => (
                  <TaskCard
                    key={task.id}
                    task={task}
                    activeTimer={activeTimer}
                  />
                ))}
              </SortableContext>
            </DroppableColumn>
          );
        })}
      </div>

      <DragOverlay>
        {activeTask ? (
          <TaskCard task={activeTask} activeTimer={activeTimer} />
        ) : null}
      </DragOverlay>
    </DndContext>
  );
}
