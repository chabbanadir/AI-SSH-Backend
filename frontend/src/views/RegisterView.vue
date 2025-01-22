<template>
  <div
    class="relative flex items-center justify-center h-screen bg-cover bg-center"
    style="background-image: url('/public/Screen.jpg');"
  >
    <!-- Register Form -->
    <div
      class="absolute bg-gray-800 p-8 rounded-lg shadow-md"
      style="width: 49%; height: 71%; top: 9%; left: 25%;"
    >
      <h1 class="text-2xl text-white text-center mb-4">Register</h1>
      <form @submit.prevent="handleRegister" class="space-y-4">
        <input
          type="text"
          v-model="username"
          placeholder="Username"
          class="p-2 border rounded w-full bg-gray-700 text-white placeholder-gray-400"
          required
        />
        <input
          type="email"
          v-model="email"
          placeholder="Email"
          class="p-2 border rounded w-full bg-gray-700 text-white placeholder-gray-400"
          required
        />
        <input
          type="password"
          v-model="password"
          placeholder="Password (min. 6 characters)"
          class="p-2 border rounded w-full bg-gray-700 text-white placeholder-gray-400"
          minlength="6"
          required
        />
        <button type="submit" class="bg-blue-500 text-white py-2 rounded w-full">
          Register
        </button>
      </form>
      <p class="text-gray-400 text-center mt-4">
        Already have an account?
        <router-link to="/login" class="text-blue-400 hover:underline">Login</router-link>
      </p>
    </div>
  </div>
</template>

<script lang="ts">
import axiosInstance from "../plugins/axios";

export default {
  name: "RegisterView",
  data() {
    return {
      username: "",
      email: "",
      password: "",
    };
  },
  methods: {
    async handleRegister() {
      try {
        await axiosInstance.post("/users/register", {
          userName: this.username,
          email: this.email,
          password: this.password,
        });
        alert("Registration successful. Please login.");
        this.$router.push("/login");
        }catch (error) {
          const err = error as ApiError; // Cast error to the ApiError type
          console.error("Registration error:", err);
          alert(err.response?.data?.message || "Registration failed. Please try again.");
        }
    },
  },
};

interface ApiError {
  response?: {
    data?: {
      message?: string;
    };
  };
}
</script>
