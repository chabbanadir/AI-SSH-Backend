<template>
    <div class="flex items-center justify-between bg-gray-800 text-white border-top rounded-t-xl px-4 py-2  shadow-md">
      <!-- Left Slot for Title -->
      <div class="flex items-center space-x-2">
        <span class="text-md font-bold">  $ > SSH TERMINAL AI POWERED ©</span>
      </div>
  
   
  
      <!-- Right Slot for Icons -->
      <div class="flex items-center space-x-4">
        <!-- Icons -->
        
        <button
          class="p-1 rounded hover:bg-gray-600 focus:outline-none focus:ring-2 focus:ring-red-600"
          aria-label="Icon 3"
          @click="$emit('close')"
        >
        <Icon icon="mingcute:close-fill" width="20" height="20" class="text-red-600" />
        </button>
      </div>
    </div>
  </template>
  
  <script lang="ts" setup>
  import { Icon } from "@iconify/vue";

  
  </script>
  
  <style scoped>
  /* Additional customization if needed */
  </style>