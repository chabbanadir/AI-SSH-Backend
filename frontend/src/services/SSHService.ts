import type { SSHConfig } from "../interfaces/SSHConfig.ts";
import axiosInstance from "../plugins/axios";

const API_BASE = "/SSHHostConfig";

export const SSHService = {
  async fetchConfigs(): Promise<SSHConfig[]> {
    const response = await axiosInstance.get(API_BASE);
    // Handle $values wrapping if needed
    return response.data.$values || response.data;
  },

  async fetchConfigById(id: string): Promise<SSHConfig> {
    const response = await axiosInstance.get(`${API_BASE}/${id}`);
    return response.data;
  },

  async createConfig(config: Omit<SSHConfig, "id">): Promise<SSHConfig> {
    const response = await axiosInstance.post(API_BASE, config);
    return response.data;
  },
  
  async updateConfig(id: string, config: Partial<SSHConfig>): Promise<SSHConfig> {
    const response = await axiosInstance.put(`${API_BASE}/${id}`, config);
    return response.data;
  },
  

  async deleteConfig(id: string): Promise<void> {
    await axiosInstance.delete(`${API_BASE}/${id}`);
  },
};

