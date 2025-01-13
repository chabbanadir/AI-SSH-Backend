<template>
    <div>
      <h1 class="text-xl font-bold mb-4">SSH Host Configurations</h1>
      <ul class="space-y-4">
        <li
          v-for="(config, index) in sshConfigs"
          :key="config.id"
          class="bg-gray-800 p-4 rounded flex justify-between items-center"
        >
          <span>{{ config.hostname }}:{{ config.port }}</span>
          <button
            @click="editConfig(index)"
            class="bg-yellow-500 text-black p-2 rounded hover:bg-yellow-600"
          >
            Edit
          </button>
        </li>
      </ul>
    </div>
  </template>
  
  <script lang="ts">
  import { defineComponent } from "vue";
  import axiosInstance from "../plugins/axios.ts";
  
  interface SSHConfig {
    id: string;
    hostname: string;
    port: number;
  }
  
  export default defineComponent({
    name: "SettingsView",
    data() {
      return {
        sshConfigs: [] as SSHConfig[],
      };
    },
    async created() {
      try {
        const response = await axiosInstance.get<SSHConfig[]>("/SSHConfigs");
        this.sshConfigs = response.data;
      } catch (error) {
        console.error("Error fetching SSH configs:", error);
      }
    },
    methods: {
      editConfig(index: number) {
        const editedHost = prompt(
          "Edit SSH Host:",
          `${this.sshConfigs[index].hostname}:${this.sshConfigs[index].port}`
        );
        if (editedHost) {
          const [hostname, port] = editedHost.split(":");
          this.sshConfigs[index].hostname = hostname;
          this.sshConfigs[index].port = parseInt(port, 10);
        }
      },
    },
  });
  </script>
  