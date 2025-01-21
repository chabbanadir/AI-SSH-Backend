import { reactive } from "vue";

export const authState = reactive({
  isAuthenticated: !!sessionStorage.getItem("sid"),
  sessionId: sessionStorage.getItem("sid") || null,
  userId: sessionStorage.getItem("userId") || null, // Store the user ID
});

export const login = (sid: string, userId: string) => {
  sessionStorage.setItem("sid", sid);
  sessionStorage.setItem("userId", userId); // Save user ID in sessionStorage
  authState.isAuthenticated = true;
  authState.sessionId = sid;
  authState.userId = userId; // Update the reactive state
};

export const logout = () => {
  sessionStorage.removeItem("sid");
  sessionStorage.removeItem("userId");
  authState.isAuthenticated = false;
  authState.sessionId = null;
  authState.userId = null;
};
