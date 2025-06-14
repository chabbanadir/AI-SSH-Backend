<template>
  <div class="p-4">
    <div class="flex justify-between items-center">
      <h1 class="text-xl font-bold mb-4">AI Interaction and Command Management</h1>
      <button
        @click="toggleDropdown"
        class="bg-gray-700 text-white p-2 rounded hover:bg-gray-600 text-sm"
      >
        {{ showDropdown ? "Hide" : "Expand" }}
      </button>
    </div>

    <div v-if="showDropdown" class="mb-4">
      <label for="sshHostConfig" class="block mb-2 text-sm font-medium">Select SSH Host Config:</label>
      <select
        v-model="selectedSshHostConfigId"
        id="sshHostConfig"
        class="block w-full p-2 bg-gray-700 text-white rounded"
      >
        <option v-for="config in sshHostConfigs" :key="config.id" :value="config.id">
          {{ config.hostname }}:{{ config.port }}
        </option>
      </select>
    </div>

    <div v-if="showDropdown" class="flex items-center justify-between mb-4">
    <button
      @click="initiateSSHSession"
      class="bg-blue-500 text-white p-2 px-4 rounded hover:bg-blue-600"
    >
      Initiate SSH Session
    </button>
    <button
      v-if="sshSessionId"
      @click="startAiConversation"
      class="bg-green-500 text-white p-2 px-4 rounded hover:bg-green-600"
    >
      Start AI Conversation
    </button>
  


</div>
<!-- 
    <div v-if="!isLoading" class="loading-spinner">
      <p>Loading Terminal...</p>
    </div>
 -->


    <TerminalComponent
      v-show="isLoading"
      :directory="initialDirectory"
      :onMessage="sendMessageToAI"
      :sshSessionId="sshSessionId || ''"
      @close="endAllSessions" 
      />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import TerminalComponent from "../components/TerminalComponent.vue";
import axiosInstance from "../plugins/axios";
import { authState } from "../stores/authStore"; // Import authState for userId

interface SSHConfig {
  id: string;
  hostname: string;
  port: number;
}
interface SSHHostConfigApiResponse {
  $id: string;
  $values: Array<{
    id: string;
    hostname: string;
    port: number;
    userId: string; // Include userId in the interface
    [key: string]: unknown;
  }>;
}

// State variables
const showDropdown = ref(true);
const sshSessionId = ref<string | null>(null);
const initialDirectory = ref("~");
const aiConversationId = ref<string | null>(null);
const terminalOutput = ref<string[]>([]);
const sshHostConfigs = ref<SSHConfig[]>([]);
const selectedSshHostConfigId = ref<string>("");
const isLoading = ref(false);

// Methods
const toggleDropdown = () => {
  showDropdown.value = !showDropdown.value;
};

const initiateSSHSession = async () => {
  try {
    const response = await axiosInstance.post("/SSHSession", {
      sshHostConfigId: selectedSshHostConfigId.value,
    });
    sshSessionId.value = response.data.id;
    initialDirectory.value = response.data.initialWorkingDirectory || "~";
  } catch (error) {
    console.error("Error initiating SSH session:", error);
  } finally {
    isLoading.value = false;
  }
};

const endAllSessions = async () => {
  let aiConversationEnded = false;
  let sshSessionEnded = false;

  if (aiConversationId.value) {
    try {
      await axiosInstance.post(`/AiConversation/${aiConversationId.value}/end`);
      aiConversationId.value = null;
      aiConversationEnded = true;
    } catch (error) {
      console.error("Error ending AI conversation:", error);
      alert("Failed to end AI conversation.");
    }
  }

  if (sshSessionId.value) {
    try {
      await axiosInstance.post(`/SSHSession/${sshSessionId.value}/end`);
      sshSessionId.value = null;
      initialDirectory.value = "~";
      sshSessionEnded = true;
    } catch (error) {
      console.error("Error ending SSH session:", error);
      alert("Failed to end SSH session.");
    }
  }
  isLoading.value = false;

  if (aiConversationEnded || sshSessionEnded) {
    console.log(
      `Sessions ended:\n${aiConversationEnded ? "AI Conversation" : ""}\n${
        sshSessionEnded ? "SSH Session" : ""
      }`
    );
  } else {
    console.log("No active sessions to end.");
  }
};

const startAiConversation = async () => {
  if (!sshSessionId.value) {
    console.error("SSH session ID is missing or not set correctly.");
    alert("SSH session not created yet!");
    return;
  }

  isLoading.value = true;

  try {
    const response = await axiosInstance.post(
      `/AiConversation/start?sshSessionId=${sshSessionId.value}`,
      null,
      {
        headers: { "Content-Type": "application/json" },
      }
    );

    aiConversationId.value = response.data.conversationId;

    console.log("AI Conversation started:", response.data);
  } catch (error) {
    console.error("Error starting AI conversation:", error);
  }
};

const sendMessageToAI = async (message: string) => {
  if (!aiConversationId.value) {
    console.error("AI conversation is not started.");
    return;
  }

  try {
    terminalOutput.value.push(`${initialDirectory.value}$ ${message}`);

    const response = await axiosInstance.post(
      `/AiConversation/${aiConversationId.value}/messages`,
      { message }
    );

    const aiAnswer = response.data.answer;
    terminalOutput.value.push(aiAnswer);

    return response.data.aiMessage;
  } catch (error) {
    console.error("Error sending message to AI:", error);
    return "Error: Could not get AI response.";
  }
};

// Fetch SSH host configurations on mount
onMounted(async () => {
  try {
    const response = await axiosInstance.get<SSHHostConfigApiResponse>(
      "/SSHHostConfig"
    );
    if (response.data.$values.length === 0) {
      alert("No SSH host configurations found.");
      return;
    }
    // Filter configurations by the logged-in user's ID
    sshHostConfigs.value = response.data.$values
      .filter((item) => item.userId === authState.userId) // Filter by userId
      .map((item) => ({
        id: item.id,
        hostname: item.hostname,
        port: item.port,
      }));
    selectedSshHostConfigId.value = sshHostConfigs.value[0]?.id || "";
  } catch (error) {
    console.error("Error fetching SSH host configurations:", error);
    alert("Failed to fetch SSH configurations.");
  }
});
</script>

<style>
.loading-spinner {
  text-align: center;
  font-size: 16px;
  color: #4a4a4a;
  margin: 20px 0;
}
</style>
