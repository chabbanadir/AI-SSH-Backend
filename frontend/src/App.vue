<template>
  <div class="flex h-screen">
    <!-- Sidebar Navigation -->
    <div
      :class="`transition-all duration-300 bg-gray-800 text-white ${
        isSidebarExpanded ? 'w-1/6 p-4' : 'w-16 p-2'
      }`"
    >
      <!-- Header Section -->
      <div class="flex items-center justify-between mb-4">
        <h2 class="text-lg font-bold" v-if="isSidebarExpanded">Navigation</h2>
        <button
          @click="toggleSidebar"
          class="text-center bg-gray-700 hover:bg-gray-600 text-white p-2 rounded"
        >
          <font-awesome-icon
            :icon="[isSidebarExpanded ? 'fas' : 'fas', isSidebarExpanded ? 'chevron-left' : 'bars']"
            class="text-lg"
          />
        </button>
      </div>

      <!-- Navigation Links -->
      <ul class="space-y-2">
        <li>
          <router-link
            to="/"
            class="flex items-center p-2 rounded hover:bg-gray-700"
          >
            <font-awesome-icon :icon="['fas', 'home']" class="mr-2" />
            <span v-if="isSidebarExpanded">Home</span>
          </router-link>
        </li>
        <li>
          <router-link
            to="/Setting"
            class="flex items-center p-2 rounded hover:bg-gray-700"
          >
            <font-awesome-icon :icon="['fas', 'cog']" class="mr-2" />
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
/* No changes needed for styles */
</style>
