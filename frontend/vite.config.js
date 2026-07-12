/// <reference types="vitest" />
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    host: true,
    proxy: {
      '/api': {
        target: 'https://localhost:7273',
        changeOrigin: true,
        secure: false
      }
    }
  },
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: './src/setupTests.jsx',
    include: ['src/**/*.{test,spec}.{js,jsx}'],
    
    reporters: ['default', 'html'], 
    outputFile: './html-report/index.html', // Шлях, куди Vitest збереже красиву сторінку з результатами
  }
})
