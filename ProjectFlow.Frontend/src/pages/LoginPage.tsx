import { useState } from "react";
import { useMutation } from "@tanstack/react-query";
import { authService } from "../services/authService";
import type { LoginRequest } from "../services/authService";
import { api } from "../services/api";

export default function LoginPage() {
  const [formData, setFormData] = useState<LoginRequest>({
    email: "",
    password: "",
  });

  const loginMutation = useMutation({
    mutationFn: authService.login,
    onSuccess: (data) => {
      console.log("Login successful:", data);

      localStorage.setItem("authToken", data.token);
      localStorage.setItem("refreshToken", data.refreshToken);
      localStorage.setItem("user", JSON.stringify(data.user));

      console.log("Token saved");
    },
    onError: (error) => {
      console.error("Login error:", error);
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    loginMutation.mutate(formData);
  };

  return (
    <div style={{ padding: "20px", maxWidth: "400px", margin: "0 auto" }}>
      <h1>ProjectFlow Login</h1>

      <form onSubmit={handleSubmit}>
        <div style={{ marginBottom: "15px" }}>
          <label>Email:</label>
          <input
            type="email"
            value={formData.email}
            onChange={(e) =>
              setFormData({ ...formData, email: e.target.value })
            }
            required
            style={{ width: "100%", padding: "8px", marginTop: "5px" }}
          />
        </div>

        <div style={{ marginBottom: "15px" }}>
          <label>Password:</label>
          <input
            type="password"
            value={formData.password}
            onChange={(e) =>
              setFormData({ ...formData, password: e.target.value })
            }
            required
            style={{ width: "100%", padding: "8px", marginTop: "5px" }}
          />
        </div>

        <button
          type="submit"
          disabled={loginMutation.isPending}
          style={{
            width: "100%",
            padding: "12px",
            backgroundColor: loginMutation.isPending ? "#ccc" : "#007bff",
            color: "white",
            border: "none",
            cursor: loginMutation.isPending ? "not-allowed" : "pointer",
          }}
        >
          {loginMutation.isPending ? "Logging in..." : "Login"}
        </button>
      </form>
      <div style={{ marginTop: "20px" }}>
        <button
          onClick={() => {
            api
              .get("/users")
              .then((res) => {
                console.log(
                  "✅ Authorized request SUCCESS! Status:",
                  res.status
                );
                console.log("Users data:", res.data);
              })
              .catch((err) => {
                console.error(
                  "Request error:",
                  err.response?.status,
                  err.message
                );
              });
          }}
          style={{ padding: "8px 16px" }}
        >
          Test Authorized Request (/api/users)
        </button>
      </div>
      {loginMutation.error && (
        <div style={{ color: "red", marginTop: "15px" }}>
          Login failed: {(loginMutation.error as Error).message}
        </div>
      )}
    </div>
  );
}
