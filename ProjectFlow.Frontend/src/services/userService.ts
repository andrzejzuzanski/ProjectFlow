import { api } from "./api";

export interface User {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  role: number;
}

export const userService = {
  getAll: async (): Promise<User[]> => {
    const response = await api.get("/users");
    return response.data;
  },
};
