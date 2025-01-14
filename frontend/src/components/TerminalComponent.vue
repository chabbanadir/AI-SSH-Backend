<template>
  <div class="flex h-screen bg-gray-900 text-white">
    <!-- Terminal Section -->
    <div class="w-3/4 p-4 bg-black text-green-500 font-mono border-r border-gray-600">
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
          >
            {{ line }}
          </div>

          <!-- Current Prompt with Input -->
          <div class="flex items-center" v-if="!isProcessing">
          <span class="mr-2">{{ directory }}$</span>
          <input
            v-model="currentInput"
            @keydown.enter="handleEnter"
            class="bg-transparent outline-none text-green-500 flex-grow"
            placeholder="Type a command..."
          />
        </div>
        <div v-else class="text-green-500 italic">
          Waiting for AI response...
        </div>
        </div>

        <!-- White Space at the Bottom -->
        <div class="flex-grow"></div>
      </div>
    </div>

    <!-- Active Commands Queue Section -->
    <div class="w-1/4 p-4 bg-gray-800">
      <h2 class="text-lg font-bold border-b border-gray-600 pb-2 mb-4">
        Active Commands Queue
      </h2>
      <ul class="space-y-2">
        <li
          v-for="(command, index) in commandsQueue"
          :key="index"
          class="flex justify-between items-center bg-gray-700 p-2 rounded hover:bg-gray-600"
        >
          <span>{{ command }}</span>
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
            <!-- Add Button -->
            <button
              @click="addCommandToTerminal(index)"
              class="bg-green-500 text-white rounded p-1 hover:bg-green-600"
              title="Add to Terminal"
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
    required: false,
  },
  sshSessionId: {
    type: String,
    required: true, // Mark it as required if necessary
    default: "", // Default to an empty string if null

  },
});

// Refs
const output = ref<string[]>([]); // Terminal output history
const currentInput = ref<string>(""); // Current input
const commandsQueue = ref<string[]>([]); // Active commands queue
const newCommand = ref<string>(""); // New command input
const isProcessing = ref(false);

// Methods
const handleEnter = async () => {
  if (!currentInput.value.trim()) return;

  const inputCommand = currentInput.value.trim();
  output.value.push(`${props.directory}$ ${inputCommand}`);
  currentInput.value = "";

  isProcessing.value = true; // Start "loading" state

  try {
    if (props.onMessage) {
      // Wait for the AI response
      const response = await props.onMessage(inputCommand);

      // Extract the JSON-like content
      const jsonMatch = response.match(/```json\n([\s\S]*?)```/);
      if (jsonMatch && jsonMatch[1]) {
        try {
          // Parse the extracted JSON
          const parsedResponse = JSON.parse(jsonMatch[1]);
          const details = parsedResponse.details;
          const commands = parsedResponse.Commands;

          // Add `details` to terminal output
          output.value.push(details);

          // Add each command to the commandsQueue
          commands.forEach((command: string) => {
            if (!commandsQueue.value.includes(command)) {
              commandsQueue.value.push(command);
            }
          });
        } catch (parseError) {
          output.value.push("Error: Unable to parse AI response.");
          console.error("JSON parse error:", parseError);
        }
      } else {
        output.value.push("Error: No valid JSON found in AI response.");
      }
    } else {
      output.value.push("Simulated command output...");
    }
  } catch (error) {
    output.value.push("Error processing the command.");
    console.error("Error in handleEnter:", error);
  } finally {
    isProcessing.value = false; // Release "loading" state
  }
};



const addNewCommand = () => {
  if (newCommand.value.trim()) {
    commandsQueue.value.push(newCommand.value.trim());
    newCommand.value = "";
  }
};

const deleteCommand = (index: number) => {
  commandsQueue.value.splice(index, 1);
};

const editCommand = (index: number) => {
  const editedCommand = prompt("Edit Command:", commandsQueue.value[index]);
  if (editedCommand && editedCommand.trim()) {
    commandsQueue.value[index] = editedCommand.trim();
  }
};
const addCommandToTerminal = async (index: number) => {
  const command = commandsQueue.value[index];
  output.value.push(`${props.directory}$ ${command}`);

  if (!props.sshSessionId) {
    output.value.push("Error: SSH session is not active.");
    return;
  }

  try {
    // Call backend API to execute the command
    const response = await axiosInstance.post(`/SSHSession/${props.sshSessionId}/ExecuteCommand`, { command });

    // Extract data from the backend response
    const { output: commandOutput, errorOutput, exitCode } = response.data;

    if (exitCode === 0) {
      // Success: Display the standard output
      output.value.push(commandOutput || "Command executed successfully.");
    } else {
      // Error: Display the error output and exit code
      output.value.push(`Error: ${errorOutput || "Unknown error occurred."}`);
      output.value.push(`Exit code: ${exitCode}`);
    }
  } catch (error) {
    output.value.push("Error executing command.");
    console.error("Error in addCommandToTerminal:", error);
  }
};

</script>
