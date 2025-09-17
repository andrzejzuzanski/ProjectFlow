import axios from "axios";

export const api = axios.create({
  baseURL: "https://localhost:7132/api",
  headers: {
    "Content-Type": "application/json",
  },
});

// Request interceptor for JWT token
api.interceptors.request.use((config) => {
  return config;
});

//Response interceptor to handle errors globally
api.interceptors.request.use(
  (response) => response,
  (error) => {
    console.error("API Error:", error);
    return Promise.reject(error);
  }
);
