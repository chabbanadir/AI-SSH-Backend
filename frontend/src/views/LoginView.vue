<template>
  <div class="flex flex-col items-center justify-center min-h-screen bg-gray-900 text-white">
    <div class="bg-gray-800 p-8 rounded-lg shadow-lg w-full max-w-md">
      <h1 class="text-2xl font-bold mb-6 text-center">Login</h1>
      <form @submit.prevent="handleLogin" class="space-y-4">
        <div>
          <label for="username" class="block text-sm mb-1">Username</label>
          <input
            id="username"
            v-model="username"
            type="text"
            placeholder="Enter your username"
            class="w-full p-3 rounded-lg border border-gray-700 bg-gray-900 text-white"
          />
        </div>
        <div>
          <label for="password" class="block text-sm mb-1">Password</label>
          <input
            id="password"
            v-model="password"
            type="password"
            placeholder="Enter your password"
            class="w-full p-3 rounded-lg border border-gray-700 bg-gray-900 text-white"
          />
        </div>
        <button
          type="submit"
          class="w-full bg-blue-500 hover:bg-blue-600 text-white font-bold py-3 rounded-lg"
        >
          Login
        </button>
      </form>
      <p class="text-center text-gray-400 mt-4">
        Don't have an account?
        <router-link to="/register" class="text-blue-400 hover:underline">Register</router-link>
      </p>
    </div>
  </div>
</template>

<script lang="ts">
import axiosInstance from "../plugins/axios";
import { login } from "../stores/authStore";

export default {
  name: "LoginView",
  data() {
    return {
      username: "",
      password: "",
    };
  },
  methods: {
    async handleLogin() {
      try {
        const response = await axiosInstance.post("/users/login", {
          userName: this.username,
          password: this.password,
        });
        if (response.status === 200 && response.data.message === "Login successful") {
          // Great: we treat that as success
          const userId = response.data.userId;
          // If you want to pass a "sessionId" as well, you could do it.
          // For now, let's store both "sid" and "userId" as userId or some placeholder
          login(userId);
          this.$router.push("/");
        } else {
          alert("Login failed: " + response.data.message);
        }
      } catch (error) {
        console.error("Login error:", error);
        alert("Invalid credentials or server error.");
      }
    },
  },
};
</script>
