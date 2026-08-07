import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";
import path from "node:path";

export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: { "@": path.resolve(__dirname, "./src") },
  },
  server: {
    // Must stay in the API's Cors:AllowedOrigins list, otherwise the browser
    // blocks every request before it reaches the backend.
    port: 5173,
  },
});
