import axios from 'axios'

// VITE_API_BASE_URL can be set to 'http://localhost:8080' for local dev,
// and to 'http://api:8080' when running inside Docker compose.
const baseURL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:8080'

export const api = axios.create({
  baseURL,
  headers: { 'Content-Type': 'application/json' }
})
