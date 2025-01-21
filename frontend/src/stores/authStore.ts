import { reactive } from "vue";

export const authState = reactive({
  isAuthenticated: false,
  userId: null as string | null,
});

export function login(userId: string) {
  authState.isAuthenticated = true;
  authState.userId = userId;
  // No need to store sid if the cookie is handled automatically
}

export function logout() {
  authState.isAuthenticated = false;
  authState.userId = null;
  // Possibly call an API endpoint to sign out / remove cookie
}
