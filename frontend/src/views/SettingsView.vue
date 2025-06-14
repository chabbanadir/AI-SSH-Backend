<template>
  <div class="p-4">
    <div class="flex justify-between">
      <h1 class="text-xl font-bold mb-4">SSH Host Configurations</h1>

      <!-- Add Host Button -->
      <button
        @click="openAddModal"
        class="bg-blue-500 text-white p-2 rounded hover:bg-blue-600 mb-4"
      >
        Add Host Config
      </button>
    </div>
    <!-- List of Configurations -->
    <ul v-if="sshConfigs.length > 0" class="space-y-4">
      <li
        v-for="(config) in sshConfigs"
        :key="config.id"
        class="bg-gray-800 p-4 rounded flex justify-between items-center border border-gray-700"
      >
        <div class="text-white">
          <p class="font-semibold">{{ config.hostname || "No hostname" }}</p>
          <p class="text-sm text-gray-400">Port: {{ config.port || "No port" }}</p>
        </div>
        <div class="flex space-x-2">
          <button
            @click="openEditModal(config)"
            class="bg-yellow-500 text-black p-2 rounded hover:bg-yellow-600"
          >
            Edit
          </button>
          <button
            @click="deleteConfig(config.id)"
            class="bg-red-500 text-white p-2 rounded hover:bg-red-600"
          >
            Delete
          </button>
        </div>
      </li>
    </ul>
    <p v-else class="text-gray-400">No SSH configurations found.</p>

    <!-- Modal for Add/Edit -->
    <div
      v-if="isModalOpen"
      class="fixed top-0 left-0 w-full h-full flex items-center justify-center bg-black bg-opacity-50"
    >
      <div class="bg-gray-900 p-6 rounded shadow-lg w-96 text-white">
        <h2 class="text-lg font-bold mb-4">
          {{ editingConfig ? "Edit Host Configuration" : "Add Host Configuration" }}
        </h2>

        <form @submit.prevent="handleSave">
          <div class="mb-4">
            <label for="hostname" class="block text-sm font-bold mb-1 text-gray-300">Hostname:</label>
            <input
              v-model="modalData.hostname"
              id="hostname"
              type="text"
              class="w-full p-2 border rounded bg-gray-800 text-white placeholder-gray-400"
              placeholder="Enter hostname"
              required
            />
          </div>
          <div class="mb-4">
            <label for="port" class="block text-sm font-bold mb-1 text-gray-300">Port:</label>
            <input
              v-model.number="modalData.port"
              id="port"
              type="number"
              class="w-full p-2 border rounded bg-gray-800 text-white placeholder-gray-400"
              placeholder="22"
              required
            />
          </div>
          <div class="mb-4">
            <label for="authType" class="block text-sm font-bold mb-1 text-gray-300">Auth Type:</label>
            <select
              v-model="modalData.authType"
              id="authType"
              class="w-full p-2 border rounded bg-gray-800 text-white"
            >
              <option value="password">Password</option>
              <option value="key">Key</option>
            </select>
          </div>
          <div class="mb-4">
            <label for="username" class="block text-sm font-bold mb-1 text-gray-300">Username:</label>
            <input
              v-model="modalData.username"
              id="username"
              type="text"
              class="w-full p-2 border rounded bg-gray-800 text-white placeholder-gray-400"
              placeholder="Enter username"
              required
            />
          </div>

          <div class="mb-4">
            <label for="passwordOrKeyPath" class="block text-sm font-bold mb-1 text-gray-300">
              Password/Key Path:
            </label>
            <input
              v-model="modalData.passwordOrKeyPath"
              id="passwordOrKeyPath"
              type="text"
              class="w-full p-2 border rounded bg-gray-800 text-white placeholder-gray-400"
              placeholder="Enter password or key path"
              required
            />
          </div>
          <div class="mb-4 flex items-center">
            <input
              v-model="modalData.sshDefaultConfig"
              id="sshDefaultConfig"
              type="checkbox"
              class="mr-2"
            />
            <label for="sshDefaultConfig" class="text-gray-300">Set as default configuration</label>
          </div>
          <div class="flex justify-end space-x-4">
            <button
              type="button"
              @click="closeModal"
              class="bg-gray-700 text-white px-4 py-2 rounded hover:bg-gray-600"
            >
              Cancel
            </button>
            <button
              type="submit"
              class="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600"
            >
              Save
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent } from "vue";
import { SSHService } from "../services/SSHService";
import { authState } from "../stores/authStore"; // Import authState
import type { SSHConfig } from "../interfaces/SSHConfig";

export default defineComponent({
  name: "SettingsView",
  data() {
    return {
      sshConfigs: [] as SSHConfig[],
      isModalOpen: false,
      editingConfig: false,
      modalData: {
        id: null,
        hostname: "",
        port: 22,
        authType: "password",
        passwordOrKeyPath: "",
        sshDefaultConfig: false,
        username: "",
      } as unknown as Partial<SSHConfig>,
    };
  },
  async created() {
    try {
      // Fetch and filter SSHConfigs by the current user's ID
      const allConfigs = await SSHService.fetchConfigs();
      this.sshConfigs = allConfigs.filter(
        (config: SSHConfig) => config.userId === authState.userId
      );
    } catch (error) {
      console.error("Error fetching SSH configs:", error);
    }
  },
  methods: {
    openEditModal(config: SSHConfig) {
      this.editingConfig = true;
      this.modalData = { ...config };
      this.isModalOpen = true;
    },
    openAddModal() {
      this.editingConfig = false;
      this.modalData = {
        id: "",
        hostname: "",
        port: 22,
        authType: "password",
        passwordOrKeyPath: "",
        sshDefaultConfig: false,
        username: "",
      };
      this.isModalOpen = true;
    },
    closeModal() {
      this.isModalOpen = false;
      this.modalData = {
        id: "",
        hostname: "",
        port: 22,
        authType: "password",
        passwordOrKeyPath: "",
        sshDefaultConfig: false,
        username: "",
      };
    },
    async handleSave() {
      try {
        const configData: SSHConfig = {
          id: this.modalData.id || "", // Ensure id is sent when editing
          hostname: this.modalData.hostname || "",
          port: this.modalData.port || 22,
          authType: this.modalData.authType || "password",
          passwordOrKeyPath: this.modalData.passwordOrKeyPath || "",
          sshDefaultConfig: this.modalData.sshDefaultConfig || false,
          username: this.modalData.username || "",
          userId: authState.userId || "", // Dynamically use the userId from authState
        };

        if (this.editingConfig && this.modalData.id) {
          // Update existing config
          await SSHService.updateConfig(this.modalData.id, configData);
        } else {
          // Create new config
          await SSHService.createConfig(configData);
        }

        // Fetch and filter updated configurations
        const allConfigs = await SSHService.fetchConfigs();
        this.sshConfigs = allConfigs.filter(
          (config: SSHConfig) => config.userId === authState.userId
        );

        this.closeModal();
      } catch (error) {
        console.error("Error saving SSH config:", error);
      }
    },
    async deleteConfig(id: string) {
      if (confirm("Are you sure you want to delete this configuration?")) {
        try {
          await SSHService.deleteConfig(id);
          this.sshConfigs = this.sshConfigs.filter((config) => config.id !== id);
        } catch (error) {
          console.error("Error deleting SSH config:", error);
        }
      }
    },
  },
});
</script>
