import { configDefaults, defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'

// PERF-01: vendors change far less often than app code, so they get their own long-cached chunks.
const vendorChunks: Record<string, RegExp> = {
  react: /node_modules\/(react|react-dom|react-router|react-router-dom|scheduler)\//,
  mui: /node_modules\/(@mui|@emotion|@popperjs|react-transition-group|stylis)\//,
  tanstack: /node_modules\/@tanstack\//,
  forms: /node_modules\/(react-hook-form|@hookform|zod)\//,
  i18n: /node_modules\/(i18next|react-i18next)\//,
}

export default defineConfig({
  plugins: [react()],
  build: {
    rollupOptions: {
      output: {
        manualChunks(id) {
          const path = id.replace(/\\/g, '/')
          return Object.keys(vendorChunks).find((name) => vendorChunks[name].test(path))
        },
      },
    },
  },
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: process.env.VITE_DEV_API_URL ?? 'http://localhost:8080',
        changeOrigin: true,
      },
    },
  },
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['./src/test/setup.ts'],
    css: true,
    restoreMocks: true,
    exclude: [...configDefaults.exclude, 'e2e/**'],
  },
})
