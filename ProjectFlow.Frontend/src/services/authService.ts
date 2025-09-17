import { api } from "./api.ts";

export interface RegisterRequest {
  email: string;
  firstName: string;
  lastName: string;
  password: string;
  confirmPassword: string;
  role: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  expiresAt: string;
  user: {
    id: number;
    email: string;
    firstName: string;
    lastName: string;
    role: string;
  };
}

export const authService = {
  login: async (credentials: LoginRequest): Promise<LoginResponse> => {
    const response = await api.post("/auth/login", credentials);
    return response.data;
  },
  register: async (userData: RegisterRequest): Promise<LoginResponse> => {
    const response = await api.post("/auth/register", userData);
    return response.data;
  },
};
