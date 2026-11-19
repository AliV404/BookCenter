import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      // In development, /api/* is forwarded to the ASP.NET Core backend
      // (the "http" profile in presentation/BookCenter.API/Properties/launchSettings.json).
      '/api': { target: 'http://localhost:5209', changeOrigin: true },
    },
  },
})
