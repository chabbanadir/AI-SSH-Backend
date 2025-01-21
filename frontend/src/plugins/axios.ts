// axios.ts
import axios from "axios";
import { authState } from "../stores/authStore";

const axiosInstance = axios.create({
  baseURL: "http://localhost:5000/api",
  timeout: 10000,
  headers: {
    "Content-Type": "application/json",
  },
});

axiosInstance.interceptors.request.use((config) => {
  const sid = authState.sessionId;
  const userId = authState.userId;
  if (sid) {
    config.headers["Authorization"] = `Session ${sid}`;
  }
  if (userId) {
    config.headers["UserId"] = userId; // Example: Send userId in headers
  }
  return config;
});

export default axiosInstance;
