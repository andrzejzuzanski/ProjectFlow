export interface Toast {
  id: string;
  type: "success" | "info" | "warning" | "error";
  title: string;
  message: string;
  duration?: number;
  timestamp: Date;
}

class ToastService {
  private toasts: Toast[] = [];
  private listeners: ((toasts: Toast[]) => void)[] = [];

  private generateId(): string {
    return Math.random().toString(36).substr(2, 9);
  }

  private notify(): void {
    this.listeners.forEach((listener) => listener([...this.toasts]));
  }

  show(toast: Omit<Toast, "id" | "timestamp">): void {
    const newToast: Toast = {
      ...toast,
      id: this.generateId(),
      timestamp: new Date(),
      duration: toast.duration ?? 4000,
    };

    this.toasts.push(newToast);
    this.notify();

    // Auto-remove after duration
    if (newToast.duration && newToast.duration > 0) {
      setTimeout(() => {
        this.remove(newToast.id);
      }, newToast.duration);
    }
  }

  remove(id: string): void {
    this.toasts = this.toasts.filter((toast) => toast.id !== id);
    this.notify();
  }

  // Convenience methods
  success(title: string, message: string): void {
    this.show({ type: "success", title, message });
  }

  info(title: string, message: string): void {
    this.show({ type: "info", title, message });
  }

  warning(title: string, message: string): void {
    this.show({ type: "warning", title, message });
  }

  error(title: string, message: string): void {
    this.show({ type: "error", title, message, duration: 6000 });
  }

  // Subscribe to toast changes
  subscribe(listener: (toasts: Toast[]) => void): () => void {
    this.listeners.push(listener);

    // Return unsubscribe function
    return () => {
      this.listeners = this.listeners.filter((l) => l !== listener);
    };
  }

  clear(): void {
    this.toasts = [];
    this.notify();
  }
}

export const toastService = new ToastService();
