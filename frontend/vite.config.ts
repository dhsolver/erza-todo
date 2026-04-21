import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  test: {
    environment: 'jsdom',
    setupFiles: './src/test/setup.ts',
  },
  server: {
    host: '0.0.0.0',
    port: 5173,
    strictPort: true,
    proxy: {
      '/api': {
        target: 'https://localhost:7287',
        changeOrigin: true,
        secure: false,
      },
      '/health': {
        target: 'https://localhost:7287',
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
