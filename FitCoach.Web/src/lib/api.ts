import axios from 'axios'

const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000/api',
  withCredentials: true,
})

// Inject JWT token on every request
api.interceptors.request.use((config) => {
  if (typeof window !== 'undefined') {
    const token = localStorage.getItem('fitcoach_token')
    if (token) config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// Auto-logout on 401
api.interceptors.response.use(
  (res) => res,
  (err) => {
    if (err.response?.status === 401 && typeof window !== 'undefined') {
      localStorage.removeItem('fitcoach_token')
      localStorage.removeItem('fitcoach_user')
      window.location.href = '/login'
    }
    return Promise.reject(err)
  }
)

// ─── Types ────────────────────────────────────────────────────────────────────

export interface User { id: string; name: string; email: string; role: string; avatarUrl?: string }
export interface AuthResponse { token: string; refreshToken: string; user: User }

export interface StudentProfile {
  id: string; user: User; trainerId: string; trainerName: string
  birthDate?: string; weightKg?: number; heightCm?: number; goal?: string
  enrolledAt: string; totalSessions: number
}

export interface Exercise {
  id: string; name: string; muscleGroup: string; equipment: string
  instructions?: string; videoUrl?: string; thumbnailUrl?: string; isGlobal: boolean
}

export interface PlanExercise {
  id: string; exercise: Exercise; sets: number; reps: string
  weightKg?: number; restSeconds: number; orderIndex: number; coachNotes?: string
}

export interface WorkoutDay {
  id: string; dayOfWeek: number; label: string; notes?: string
  orderIndex: number; exercises: PlanExercise[]
}

export interface WorkoutPlan {
  id: string; name: string; description?: string
  studentId: string; studentName: string
  startDate?: string; endDate?: string; isActive: boolean
  days: WorkoutDay[]; createdAt: string
}

export interface WorkoutPlanSummary {
  id: string; name: string; description?: string
  studentId: string; studentName: string
  isActive: boolean; totalDays: number; createdAt: string
}

export interface SessionSet {
  id: string; planExerciseId: string; exerciseName: string
  setNumber: number; repsDone: number; weightKg: number; loggedAt: string
}

export interface WorkoutSession {
  id: string; workoutDayId: string; workoutDayLabel: string
  startedAt: string; finishedAt?: string; durationSeconds?: number
  avgHeartRate?: number; caloriesBurned?: number; studentNotes?: string
  sets: SessionSet[]
}

export interface StudentActivity {
  studentId: string; studentName: string; avatarUrl?: string
  lastSessionAt?: string; sessionsThisMonth: number; isActive: boolean
}

export interface TrainerDashboard {
  totalStudents: number; activeStudentsThisWeek: number
  totalPlans: number; studentActivity: StudentActivity[]
}

// ─── Auth API ─────────────────────────────────────────────────────────────────

export const authApi = {
  login: (email: string, password: string) =>
    api.post<AuthResponse>('/auth/login', { email, password }).then(r => r.data),

  registerTrainer: (data: { name: string; email: string; password: string; specialty?: string; crefNumber?: string }) =>
    api.post<AuthResponse>('/auth/register/trainer', data).then(r => r.data),

  registerStudent: (data: { name: string; email: string; password: string; trainerInviteCode: string }) =>
    api.post<AuthResponse>('/auth/register/student', data).then(r => r.data),
}

// ─── Students API ─────────────────────────────────────────────────────────────

export const studentsApi = {
  list: () => api.get<StudentProfile[]>('/students').then(r => r.data),
  get: (id: string) => api.get<StudentProfile>(`/students/${id}`).then(r => r.data),
  getActivity: (id: string) => api.get(`/students/${id}/activity`).then(r => r.data),
}

// ─── Exercises API ────────────────────────────────────────────────────────────

export const exercisesApi = {
  list: (params?: { muscle?: string; equipment?: string }) =>
    api.get<Exercise[]>('/exercises', { params }).then(r => r.data),

  create: (data: { name: string; muscleGroup: string; equipment: string; instructions?: string; videoUrl?: string }) =>
    api.post<Exercise>('/exercises', data).then(r => r.data),
}

// ─── Plans API ────────────────────────────────────────────────────────────────

export const plansApi = {
  list: () => api.get<WorkoutPlanSummary[]>('/plans').then(r => r.data),
  get: (id: string) => api.get<WorkoutPlan>(`/plans/${id}`).then(r => r.data),

  create: (data: {
    studentId: string; name: string; description?: string
    startDate?: string; endDate?: string
    days: Array<{
      dayOfWeek: number; label: string; notes?: string; orderIndex: number
      exercises: Array<{
        exerciseId: string; sets: number; reps: string
        weightKg?: number; restSeconds: number; orderIndex: number; coachNotes?: string
      }>
    }>
  }) => api.post<WorkoutPlan>('/plans', data).then(r => r.data),

  update: (id: string, data: Partial<WorkoutPlan>) =>
    api.put<WorkoutPlan>(`/plans/${id}`, data).then(r => r.data),
}

// ─── Dashboard API ────────────────────────────────────────────────────────────

export const dashboardApi = {
  trainer: () => api.get<TrainerDashboard>('/trainer/dashboard').then(r => r.data),
  student: () => api.get('/dashboard').then(r => r.data),
}

export default api
