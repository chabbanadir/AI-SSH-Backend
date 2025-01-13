<template>
  <div class="flex h-screen">
    <!-- Sidebar Navigation -->
    <div class="w-1/6 bg-gray-800 text-white p-4">
      <h2 class="text-lg font-bold mb-4">Navigation</h2>
      <ul class="space-y-2">
        <li>
          <router-link to="/" class="block p-2 rounded hover:bg-gray-700">Home</router-link>
        </li>
        <li>
          <router-link to="/settings" class="block p-2 rounded hover:bg-gray-700">Settings</router-link>
        </li>
      </ul>
    </div>

    <!-- Main Content -->
    <div class="w-5/6 p-4">
      <router-view />
    </div>
  </div>
</template>

<script  lang="ts">
export default {
  name: "App",
};
</script>
