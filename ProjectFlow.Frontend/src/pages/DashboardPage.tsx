import { useAuth } from "../contexts/AuthContext";

export default function DashboardPage() {
  const { user, logout } = useAuth();

  const getRoleName = (role: number | string | undefined) => {
    if (role === 0 || role === "0") return "Admin";
    if (role === 1 || role === "1") return "ProjectManager";
    if (role === 2 || role === "2") return "Developer";
    return "User";
  };

  return (
    <div style={{ padding: "20px" }}>
      <div
        style={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          marginBottom: "20px",
        }}
      >
        <h1>ProjectFlow Dashboard</h1>
        <button
          onClick={logout}
          style={{
            padding: "8px 16px",
            backgroundColor: "#dc3545",
            color: "white",
            border: "none",
            cursor: "pointer",
          }}
        >
          Logout
        </button>
      </div>

      <div
        style={{
          backgroundColor: "#f8f9fa",
          padding: "15px",
          borderRadius: "5px",
        }}
      >
        <h3>
          Welcome back, {user?.firstName} {user?.lastName}!
        </h3>
        <p>
          <strong>Email:</strong> {user?.email}
        </p>
        <p>
          <strong>Role:</strong> {getRoleName(user?.role)}
        </p>
        <p>
          <strong>User ID:</strong> {user?.id}
        </p>
      </div>

      <div style={{ marginTop: "20px" }}>
        <h3>Quick Stats</h3>
        <p>Dashboard is ready for project management features!</p>
        <p>Next: Projects list, Kanban board, user management</p>
      </div>
    </div>
  );
}
