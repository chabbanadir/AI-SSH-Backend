import axios from "axios";

const axiosInstance = axios.create({
  baseURL: "http://localhost:5000/api",
  timeout: 10000,
  headers: {
    "Content-Type": "application/json",
  },
});

axiosInstance.interceptors.response.use(
  (response) => response,
  (error) => {
    console.error("Axios Request Error:", {
      message: error.message,
      code: error.code,
      config: error.config,
      request: error.request,
      response: error.response,
    });
    return Promise.reject(error);
  }
);
export default axiosInstance;
