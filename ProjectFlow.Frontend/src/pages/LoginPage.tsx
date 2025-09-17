import { useQuery } from "@tanstack/react-query";
import { authService } from "../services/authService";

export default function LoginPage() {
  const { data, isLoading, error } = useQuery({
    queryKey: ["test-api"],
    queryFn: async () => {
      try {
        await fetch("https://localhost:7132/api/users");
        return "API connected";
      } catch (err) {
        throw new Error(`API connection failed ${err}`);
      }
    },
  });

  return (
    <div style={{ padding: "20px" }}>
      <h1>ProjectFlow Login</h1>

      <div>
        <h3>API Connection Test:</h3>
        {isLoading && <p>Testing API connection...</p>}
        {error && (
          <p style={{ color: "red" }}>Error: {(error as Error).message}</p>
        )}
        {data && <p style={{ color: "green" }}>{data}</p>}
      </div>
    </div>
  );
}
