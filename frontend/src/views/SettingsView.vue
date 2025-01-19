<template>
  <div>
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
        <button
          @click="openEditModal(config)"
          class="bg-yellow-500 text-black p-2 rounded hover:bg-yellow-600"
        >
          Edit
        </button>
      </li>
    </ul>
    <p v-else class="text-gray-400">No SSH configurations found.</p>

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
      this.sshConfigs = await SSHService.fetchConfigs();
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
        username: "", // Pre-fill default username
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
      if (this.editingConfig && this.modalData.id) {
        // Update Config
        const updatedConfig = await SSHService.updateConfig(this.modalData.id, {
          id: this.modalData.id!, // Include the id explicitly
          hostname: this.modalData.hostname!,
          port: this.modalData.port!,
          authType: this.modalData.authType!,
          passwordOrKeyPath: this.modalData.passwordOrKeyPath!,
          sshDefaultConfig: this.modalData.sshDefaultConfig!,
          username: this.modalData.username!, // Include username
          userId: "e36cd350-0621-492a-b350-07689e6c615a", // Ensure UserId is sent
        });
        // Find and update in the list
        const index = this.sshConfigs.findIndex((config) => config.id === updatedConfig.id);
        if (index !== -1) this.sshConfigs[index] = updatedConfig;
      } else {
        // Add New Config
        const newConfig = await SSHService.createConfig({
          hostname: this.modalData.hostname!,
          port: this.modalData.port!,
          authType: this.modalData.authType!,
          passwordOrKeyPath: this.modalData.passwordOrKeyPath!,
          sshDefaultConfig: this.modalData.sshDefaultConfig!,
          username: this.modalData.username!,
          userId: "e36cd350-0621-492a-b350-07689e6c615a", // Fixed userId for now
        });
        this.sshConfigs.push(newConfig);
      }
      this.closeModal();
    } catch (error) {
      console.error("Error saving SSH config:", error);
    }
  },
  },
});
</script>