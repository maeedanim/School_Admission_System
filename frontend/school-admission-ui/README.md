# School Admission System — Frontend

React and TypeScript frontend for the School Admission System.

The application provides separate student and administrator experiences and communicates with the ASP.NET Core Web API.

## Live Application

- **Frontend:** https://school-admission-system-lemon.vercel.app/classes
- **Backend API:** https://school-admission-system-t609.onrender.com
- **Repository:** https://github.com/maeedanim/School_Admission_System

## Technology Stack

- React 19
- TypeScript
- Vite
- React Router
- TanStack Query
- Axios
- Lucide React

## Prerequisites

- Node.js
- npm
- A running School Admission System backend API

## Setup

From this directory:

```powershell
npm install
```

Create or configure `.env.development`:

```env
VITE_API_BASE_URL=http://localhost:8080
```

Start the development server:

```powershell
npm run dev
```

Open:

http://localhost:5173

## Available Scripts

| Command | Description |
|---|---|
| `npm run dev` | Start the Vite development server |
| `npm run build` | Run TypeScript checks and create a production build |
| `npm run lint` | Run ESLint |
| `npm run preview` | Preview the production build locally |

## Application Routes

| Route | Access | Description |
|---|---|---|
| `/login` | Public | Student login |
| `/register` | Public | Student registration |
| `/admin/login` | Public | Administrator login |
| `/classes` | Student and Admin | Browse classes |
| `/my-applications` | Student | View personal applications |
| `/admin/classes` | Admin | Manage classes |
| `/admin/applications` | Admin | View admission applications |

## API Configuration

The frontend reads its API base URL from:

```env
VITE_API_BASE_URL=http://localhost:8080
```

For production, configure the variable in the Vercel project settings:

```env
VITE_API_BASE_URL=https://school-admission-system-t609.onrender.com
```

Vite embeds environment variables into the frontend build. Rebuild or redeploy after changing production environment variables.

## Authentication

The API returns JWT access tokens for authenticated requests. Refresh tokens are managed through an HttpOnly cookie.

Requests that need the refresh cookie must use credentialed HTTP requests. The backend must allow the frontend origin through its `FRONTEND_ORIGIN` configuration and CORS policy.

Frontend route protection improves navigation and user experience. Authorization is independently enforced by the backend API.

## Production

- Frontend: https://school-admission-system-lemon.vercel.app/classes
- Backend API: https://school-admission-system-t609.onrender.com
- Repository: https://github.com/maeedanim/School_Admission_System
