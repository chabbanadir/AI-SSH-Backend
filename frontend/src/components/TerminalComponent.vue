<template >
  <div class="flex flex-row w-full space-x-2">
    <div class="w-3/4 ">
      <topBar @close="()=> {
        $emit('close');
        output = [];
      }" ></topBar>
    <!-- Terminal Section -->
    <div class="p-4 bg-black font-mono ">
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
    </div>
    <div class=" w-1/4">
      

    <!-- Active Commands Queue Section -->
    <div class=" p-4 bg-gray-800 rounded-lg">
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
        <ul v-show="activeCommandsVisible" class="space-y-1">
          <li
            v-for="(commandObj, index) in activeCommands"
            :key="index"
            class="flex justify-between items-center bg-gray-700 p-2 rounded hover:bg-gray-600 relative"
          >
            <!-- Command with Tooltip -->
            <span class="group relative text-white cursor-pointer underline  truncate ...">
              {{ commandObj.command }}
              <!-- Tooltip -->
              <span
                class="absolute left-0 bottom-full mb-2 hidden group-hover:block bg-gray-700 text-white text-sm p-auto  rounded shadow-lg max-w-xs"
              >
                {{ commandObj.explanation }}
              </span>
            </span>
            <div class="flex space-x-2">
              <div>
                <!-- Delete Button -->
             

                <button
                @click="deleteCommand(index)"
                class=" text-white bg-transparent  hover:text-red-600"
                title="Delete"
              >
              <Icon icon="mdi:delete"  width="23" height="20"/>
            </button>
              <!-- Edit Button -->
              <button
                @click="editCommand(index)"
                class="  bg-transparent  hover:text-blue-600"
                title="Edit"
              >
              <Icon icon="mdi:file-edit" width="23" height="20"/>

              </button>
              <!-- Execute Button -->
              <button
                @click="executeCommand(index)"
                class="  bg-transparent  hover:text-green-600"
                title="Execute Command"
              >
              <Icon icon="mynaui:terminal-solid" width="23" height="20"/>

              </button>
            </div>
            </div>
            
          </li>
        </ul>

        <!-- Add New Command -->
        <div class="mt-4">
          <input
            v-model="newCommand.command"
            placeholder="New Command"
            class="w-full bg-gray-600 text-white p-2 rounded mb-2 outline-none"
          />
          <textarea
            v-model="newCommand.explanation"
            placeholder="Command explanation"
            class="w-full bg-gray-600 text-white p-2 rounded mb-2 outline-none"
          ></textarea>
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
            v-for="(commandObj, index) in executedCommands"
            :key="index"
            class="flex justify-between items-center bg-gray-600 p-2 rounded"
          >
            <span class="truncate">{{ commandObj.command }}</span>
          </li>
        </ul>
      </div>
    </div>
    </div>
  </div>


 

</template>

<script setup lang="ts">
import axiosInstance from "@/plugins/axios";
import { ref } from "vue";
import { Icon } from "@iconify/vue";
import topBar from "./TopBar.vue";
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
const activeCommands = ref<{ command: string; explanation: string }[]>([]);
const executedCommands = ref<{ command: string; explanation: string }[]>([]);
const newCommand = ref<{ command: string; explanation: string }>({ command: "", explanation: "" });

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
  } catch {
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
      commands.forEach((cmd: { command: string; explanation: string }) => {
        if (!activeCommands.value.some((c) => c.command === cmd.command)) {
          activeCommands.value.push(cmd);
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
  const commandObj = activeCommands.value[index];
  output.value.push({ type: "command", content: `${props.directory}$ ${commandObj.command}` });

  try {
    const response = await axiosInstance.post(
      `/SSHSession/${props.sshSessionId}/ExecuteCommand`,
      { command: commandObj.command }
    );
    const { output: commandOutput } = response.data;

    // Display command output in terminal
    output.value.push({ type: "command", content: commandOutput });

    // Move the command to the executed list
    executedCommands.value.push(commandObj);
    activeCommands.value.splice(index, 1);
  } catch {
    output.value.push({ type: "error", content: "Error executing command." });
  }
};

const addNewCommand = () => {
  if (newCommand.value.command.trim()) {
    activeCommands.value.push({ ...newCommand.value });
    newCommand.value = { command: "", explanation: "" };
  }
};

const deleteCommand = (index: number) => {
  activeCommands.value.splice(index, 1);
};

const editCommand = (index: number) => {
  const editedCommand = prompt(
    "Edit Command:",
    activeCommands.value[index].command
  );
  const editedexplanation = prompt(
    "Edit explanation:",
    activeCommands.value[index].explanation
  );

  if (editedCommand && editedCommand.trim() && editedexplanation && editedexplanation.trim()) {
    activeCommands.value[index] = { command: editedCommand.trim(), explanation: editedexplanation.trim() };
  }
};

const toggleActiveCommands = () => {
  activeCommandsVisible.value = !activeCommandsVisible.value;
};

const toggleExecutedCommands = () => {
  executedCommandsVisible.value = !executedCommandsVisible.value;
};
</script>
