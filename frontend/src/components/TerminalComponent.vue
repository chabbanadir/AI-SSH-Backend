<template>
  <div class="flex h-screen bg-gray-900 text-white">
    <!-- Terminal Section -->
    <div class="w-3/4 p-4 bg-black font-mono border-r border-gray-600">
      <div
        ref="terminal"
        class="flex flex-col bg-black p-4 rounded resize-y overflow-auto border border-gray-600"
        style="min-height: 200px; max-height: 500px;"
      >
        <!-- Terminal Output Section -->
        <div>
          <div
            v-for="(line, index) in output"
            :key="index"
            class="whitespace-pre-line"
            :class="{
              'text-green-500': line.type === 'user',
              'text-blue-400': line.type === 'ai',
              'text-yellow-500': line.type === 'command',
              'text-red-500': line.type === 'error',
            }"
          >
            {{ line.content }}
          </div>

          <!-- Current Prompt with Input -->
          <div class="flex items-center" v-if="!isProcessing">
            <span class="mr-2">{{ directory }}$</span>
            <input
              v-model="currentInput"
              @keydown.enter="handleEnter"
              class="bg-transparent outline-none text-green-500 flex-grow border-b border-gray-600 focus:border-green-500 placeholder-italic"
              placeholder="Type a command..."
            />
          </div>
          <div v-else class="text-blue-400 italic">
            Waiting for AI response...
          </div>
        </div>

        <!-- White Space at the Bottom -->
        <div class="flex-grow"></div>
      </div>
    </div>

    <!-- Active Commands Queue Section -->
    <div class="w-1/4 p-4 bg-gray-800 rounded-lg">
      <!-- Active Commands -->
      <div>
        <h2
          @click="toggleActiveCommands"
          class="text-lg font-bold flex justify-between items-center cursor-pointer border-b border-gray-600 pb-2 mb-4"
        >
          Active Commands Queue
          <button
            class="text-sm bg-blue-500 text-white px-2 py-1 rounded hover:bg-blue-600"
          >
            {{ activeCommandsVisible ? "Hide" : "Expand" }}
          </button>
        </h2>
        <ul v-show="activeCommandsVisible" class="space-y-2">
          <li
            v-for="(command, index) in activeCommands"
            :key="index"
            class="flex justify-between items-center bg-gray-700 p-2 rounded hover:bg-gray-600"
          >
            <span class="truncate">{{ command }}</span>
            <div class="flex space-x-2">
              <!-- Delete Button -->
              <button
                @click="deleteCommand(index)"
                class="bg-red-500 text-white rounded p-1 hover:bg-red-600"
                title="Delete"
              >
                X
              </button>
              <!-- Edit Button -->
              <button
                @click="editCommand(index)"
                class="bg-yellow-500 text-black rounded p-1 hover:bg-yellow-600"
                title="Edit"
              >
                E
              </button>
              <!-- Execute Button -->
              <button
                @click="executeCommand(index)"
                class="bg-green-500 text-white rounded p-1 hover:bg-green-600"
                title="Execute Command"
              >
                +
              </button>
            </div>
          </li>
        </ul>

        <!-- Add New Command -->
        <div class="mt-4">
          <input
            v-model="newCommand"
            placeholder="New Command"
            class="w-full bg-gray-600 text-white p-2 rounded mb-2 outline-none"
          />
          <button
            @click="addNewCommand"
            class="w-full bg-blue-500 text-white p-2 rounded hover:bg-blue-600"
          >
            Add Command
          </button>
        </div>
      </div>

      <!-- Executed Commands Section -->
      <div class="mt-6">
        <h2
          @click="toggleExecutedCommands"
          class="text-lg font-bold flex justify-between items-center cursor-pointer border-b border-gray-600 pb-2 mb-4"
        >
          Executed Commands
          <button
            class="text-sm bg-blue-500 text-white px-2 py-1 rounded hover:bg-blue-600"
          >
            {{ executedCommandsVisible ? "Hide" : "Expand" }}
          </button>
        </h2>
        <ul v-show="executedCommandsVisible" class="space-y-2">
          <li
            v-for="(command, index) in executedCommands"
            :key="index"
            class="flex justify-between items-center bg-gray-600 p-2 rounded"
          >
            <span class="truncate">{{ command }}</span>
          </li>
        </ul>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import axiosInstance from "@/plugins/axios";
import { ref } from "vue";

// Props
const props = defineProps({
  directory: {
    type: String,
    default: "~",
  },
  onMessage: {
    type: Function,
    required: true,
  },
  sshSessionId: {
    type: String,
    required: true,
  },
});

// Terminal Output
const output = ref<{ type: string; content: string }[]>([]); // Terminal output
const currentInput = ref<string>(""); // Current input
const isProcessing = ref(false); // Processing state

// Command Lists
const activeCommands = ref<string[]>([]);
const executedCommands = ref<string[]>([]);
const newCommand = ref<string>("");

// Visibility Toggles
const activeCommandsVisible = ref(true);
const executedCommandsVisible = ref(false);

// Methods
const handleEnter = async () => {
  if (!currentInput.value.trim()) return;

  const inputCommand = currentInput.value.trim();
  output.value.push({ type: "user", content: `${props.directory}$ ${inputCommand}` });
  currentInput.value = "";
  isProcessing.value = true;

  try {
    const response = await props.onMessage(inputCommand);
    processAIResponse(response);
  } catch  {
    output.value.push({ type: "error", content: "Error processing the command." });
  } finally {
    isProcessing.value = false;
  }
};

const processAIResponse = (response: string) => {
  const jsonMatch = response.match(/```json\n([\s\S]*?)```/);
  if (jsonMatch && jsonMatch[1]) {
    try {
      const parsedResponse = JSON.parse(jsonMatch[1]);
      const details = parsedResponse.details;
      const commands = parsedResponse.Commands;

      // Add details to terminal output
      output.value.push({ type: "ai", content: details });

      // Add commands to the active commands queue
      commands.forEach((command: string) => {
        if (!activeCommands.value.includes(command)) {
          activeCommands.value.push(command);
        }
      });
    } catch {
      output.value.push({ type: "error", content: "Error parsing AI response." });
    }
  } else {
    output.value.push({ type: "ai", content: response });
  }
};

const executeCommand = async (index: number) => {
  const command = activeCommands.value[index];
  output.value.push({ type: "command", content: `${props.directory}$ ${command}` });

  try {
    const response = await axiosInstance.post(
      `/SSHSession/${props.sshSessionId}/ExecuteCommand`,
      { command }
    );
    const { output: commandOutput } = response.data;

    // Display command output in terminal
    output.value.push({ type: "command", content: commandOutput });

    // Move the command to the executed list
    executedCommands.value.push(command);
    activeCommands.value.splice(index, 1);
  } catch  {
    output.value.push({ type: "error", content: "Error executing command."  });
  }
};

const addNewCommand = () => {
  if (newCommand.value.trim()) {
    activeCommands.value.push(newCommand.value.trim());
    newCommand.value = "";
  }
};

const deleteCommand = (index: number) => {
  activeCommands.value.splice(index, 1);
};

const editCommand = (index: number) => {
  const editedCommand = prompt("Edit Command:", activeCommands.value[index]);
  if (editedCommand && editedCommand.trim()) {
    activeCommands.value[index] = editedCommand.trim();
  }
};

const toggleActiveCommands = () => {
  activeCommandsVisible.value = !activeCommandsVisible.value;
};

const toggleExecutedCommands = () => {
  executedCommandsVisible.value = !executedCommandsVisible.value;
};
</script>
