import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
  server: {
    // Matches one of the origins already listed in the API's Cors:AllowedOrigins.
    // Changing it means adding the new origin there too, or the browser blocks
    // every request.
    port: 5173,
  },
});
