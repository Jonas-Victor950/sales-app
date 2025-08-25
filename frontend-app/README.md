# Frontend (React + Vite + TS)

## Dev
```bash
npm i
npm run dev
```
Set `VITE_API_BASE_URL=http://localhost:8080` in a `.env` file if needed.

## Docker
In docker-compose, build with:
```yaml
web:
  build:
    context: ./frontend-app
    dockerfile: Dockerfile
    args:
      VITE_API_BASE_URL: http://api:8080
  environment:
    - VITE_API_BASE_URL=http://api:8080
  ports:
    - "5173:80"
  depends_on:
    - api
```
