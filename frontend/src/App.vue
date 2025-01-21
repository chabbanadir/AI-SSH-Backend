<script lang="ts">
import { ref } from "vue";
import { useRouter } from "vue-router"; // Import Vue Router
import { authState, logout } from "./stores/authStore";

export default {
  name: "App",
  setup() {
    const isSidebarExpanded = ref(true);
    const router = useRouter(); // Access the router instance

    const toggleSidebar = () => {
      isSidebarExpanded.value = !isSidebarExpanded.value;
    };

    const handleLogout = async () => {
      logout(); // Clear session and state
      router.push("/login"); // Redirect to login page
    };

    return {
      isSidebarExpanded,
      toggleSidebar,
      authState,
      handleLogout,
    };
  },
};
</script>
<template>
  <div class="flex h-screen">
    <!-- Sidebar Navigation -->
    <div
      v-if="authState.isAuthenticated"
      :class="`transition-all duration-300 bg-gray-800 text-white ${
        isSidebarExpanded ? 'w-1/6 p-4' : 'w-16 p-2'
      }`"
    >
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
            to="/settings"
            class="flex items-center p-2 rounded hover:bg-gray-700"
          >
            <font-awesome-icon :icon="['fas', 'cog']" class="mr-2" />
            <span v-if="isSidebarExpanded">Settings</span>
          </router-link>
        </li>
      </ul>

      <!-- Logout Button -->
      <button
        @click="handleLogout"
        class="flex items-center w-full p-2 mt-4 rounded hover:bg-red-600 bg-red-500 text-white "
      >
      <font-awesome-icon :icon="['fas', 'right-from-bracket']" class="mr-2" />
      <span v-if="isSidebarExpanded">Logout</span>
      </button>
    </div>

    <!-- Main Content -->
    <div class="flex-1 p-4">
      <router-view />
    </div>
  </div>
</template>
