<template>
  <div class="flex h-screen">
    <!-- Sidebar Navigation -->
    <div
      :class="`transition-all duration-300 bg-gray-800 text-white ${
        isSidebarExpanded ? 'w-1/6 p-4' : 'w-16 p-2'
      }`"
    >
      <button
        @click="toggleSidebar"
        class="mb-4 text-center bg-gray-700 hover:bg-gray-600 text-white p-2 rounded"
      >
        <i
          :class="isSidebarExpanded ? 'fas fa-chevron-left' : 'fas fa-bars'"
          class="text-lg"
        ></i>
      </button>
      <div v-if="isSidebarExpanded" class="mb-4">
        <h2 class="text-lg font-bold">Navigation</h2>
      </div>
      <ul class="space-y-2">
        <li>
          <router-link
            to="/"
            class="flex items-center p-2 rounded hover:bg-gray-700"
          >
            <i class="fas fa-home text-lg mr-2"></i>
            <span v-if="isSidebarExpanded">Home</span>
          </router-link>
        </li>
        <li>
          <router-link
            to="/Setting"
            class="flex items-center p-2 rounded hover:bg-gray-700"
          >
            <i class="fas fa-cog text-lg mr-2"></i>
            <span v-if="isSidebarExpanded">Settings</span>
          </router-link>
        </li>
      </ul>
    </div>

    <!-- Main Content -->
    <div class="flex-1 p-4">
      <router-view />
    </div>
  </div>
</template>

<script lang="ts">
import { ref } from "vue";

export default {
  name: "App",
  setup() {
    const isSidebarExpanded = ref(true);

    const toggleSidebar = () => {
      isSidebarExpanded.value = !isSidebarExpanded.value;
    };

    return { isSidebarExpanded, toggleSidebar };
  },
};
</script>

<style scoped>
/* Make sure Font Awesome is included */
@import url("https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.4/css/all.min.css");
</style>
