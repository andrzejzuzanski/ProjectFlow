import { useState, useEffect } from "react";
import { toastService } from "../services/toastService";
import type { Toast } from "../services/toastService";

// Individual Toast Item

function ToastItem({
  toast,
  onRemove,
}: {
  toast: Toast;
  onRemove: (id: string) => void;
}) {
  const getIcon = () => {
    switch (toast.type) {
      case "success":
        return "✅";
      case "info":
        return "ℹ️";
      case "warning":
        return "⚠️";
      case "error":
        return "❌";
      default:
        return "📢";
    }
  };

  const getBackgroundColor = () => {
    switch (toast.type) {
      case "success":
        return "#d4edda";
      case "info":
        return "#d1ecf1";
      case "warning":
        return "#fff3cd";
      case "error":
        return "#f8d7da";
      default:
        return "#e2e3e5";
    }
  };

  const getBorderColor = () => {
    switch (toast.type) {
      case "success":
        return "#c3e6cb";
      case "info":
        return "#bee5eb";
      case "warning":
        return "#ffeaa7";
      case "error":
        return "#f5c6cb";
      default:
        return "#d6d8db";
    }
  };

  return (
    <div
      style={{
        backgroundColor: getBackgroundColor(),
        border: `1px solid ${getBorderColor()}`,
        borderRadius: "8px",
        padding: "12px 16px",
        marginBottom: "8px",
        boxShadow: "0 4px 6px rgba(0, 0, 0, 0.1)",
        display: "flex",
        alignItems: "flex-start",
        gap: "12px",
        minWidth: "300px",
        maxWidth: "400px",
        animation: "slideIn 0.3s ease-out",
        position: "relative",
      }}
    >
      <span style={{ fontSize: "16px", lineHeight: "1" }}>{getIcon()}</span>

      <div style={{ flex: 1 }}>
        <div
          style={{
            fontWeight: "bold",
            fontSize: "14px",
            marginBottom: "4px",
            color: "#333",
          }}
        >
          {toast.title}
        </div>
        <div style={{ fontSize: "13px", color: "#666", lineHeight: "1.4" }}>
          {toast.message}
        </div>
        <div style={{ fontSize: "11px", color: "#999", marginTop: "4px" }}>
          {toast.timestamp.toLocaleTimeString()}
        </div>
      </div>

      <button
        onClick={() => onRemove(toast.id)}
        style={{
          background: "none",
          border: "none",
          cursor: "pointer",
          fontSize: "18px",
          color: "#999",
          lineHeight: "1",
          padding: "0",
          width: "20px",
          height: "20px",
        }}
      >
        ×
      </button>
    </div>
  );
}

export default function ToastContainer() {
  const [toasts, setToasts] = useState<Toast[]>([]);

  useEffect(() => {
    const unsubscribe = toastService.subscribe(setToasts);
    return unsubscribe;
  }, []);

  if (toasts.length === 0) return null;

  return (
    <>
      {/* CSS Animation */}
      <style>{`
        @keyframes slideIn {
          from {
            transform: translateX(100%);
            opacity: 0;
          }
          to {
            transform: translateX(0);
            opacity: 1;
          }
        }
      `}</style>

      <div
        style={{
          position: "fixed",
          top: "20px",
          right: "20px",
          zIndex: 9999,
          pointerEvents: "none",
        }}
      >
        <div style={{ pointerEvents: "auto" }}>
          {toasts.map((toast) => (
            <ToastItem
              key={toast.id}
              toast={toast}
              onRemove={toastService.remove.bind(toastService)}
            />
          ))}
        </div>
      </div>
    </>
  );
}
