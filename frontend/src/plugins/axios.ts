// axios.ts
import axios from "axios";

const axiosInstance = axios.create({
  baseURL: "http://localhost:5000/api",
  timeout: 10000,
  headers: { "Content-Type": "application/json" },
  withCredentials: true,  // crucial for cross-site cookies

});

axiosInstance.interceptors.request.use((config) => {

  return config;
});

export default axiosInstance;
