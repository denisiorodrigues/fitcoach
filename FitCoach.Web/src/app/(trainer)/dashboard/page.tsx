'use client'
import { useQuery } from '@tanstack/react-query'
import { dashboardApi, studentsApi, plansApi, type StudentActivity } from '@/lib/api'
import { Users, Dumbbell, TrendingUp, Clock, ChevronRight, Activity } from 'lucide-react'
import Link from 'next/link'
import { formatDistanceToNow } from 'date-fns'
import { ptBR } from 'date-fns/locale'

function StatCard({ label, value, icon: Icon, color }: {
  label: string; value: number | string; icon: any; color: string
}) {
  return (
    <div className="bg-white rounded-xl border border-gray-100 p-5">
      <div className={`w-10 h-10 rounded-lg ${color} flex items-center justify-center mb-3`}>
        <Icon size={20} className="text-white" />
      </div>
      <p className="text-2xl font-semibold text-gray-900">{value}</p>
      <p className="text-sm text-gray-500 mt-0.5">{label}</p>
    </div>
  )
}

function StudentRow({ activity }: { activity: StudentActivity }) {
  const initials = activity.studentName.split(' ').map(n => n[0]).join('').slice(0, 2).toUpperCase()
  return (
    <Link href={`/students/${activity.studentId}`}
      className="flex items-center justify-between py-3 px-4 hover:bg-gray-50 rounded-lg transition-colors group">
      <div className="flex items-center gap-3">
        <div className="w-9 h-9 rounded-full bg-indigo-100 flex items-center justify-center text-indigo-700 text-sm font-medium">
          {initials}
        </div>
        <div>
          <p className="text-sm font-medium text-gray-900">{activity.studentName}</p>
          <p className="text-xs text-gray-400">
            {activity.lastSessionAt
              ? formatDistanceToNow(new Date(activity.lastSessionAt), { addSuffix: true, locale: ptBR })
              : 'Nunca treinou'}
          </p>
        </div>
      </div>
      <div className="flex items-center gap-3">
        <span className={`inline-flex items-center gap-1.5 text-xs px-2 py-0.5 rounded-full font-medium ${
          activity.isActive ? 'bg-green-50 text-green-700' : 'bg-gray-100 text-gray-500'
        }`}>
          <span className={`w-1.5 h-1.5 rounded-full ${activity.isActive ? 'bg-green-500' : 'bg-gray-400'}`}/>
          {activity.isActive ? 'Ativo' : 'Inativo'}
        </span>
        <span className="text-xs text-gray-400">{activity.sessionsThisMonth} treinos</span>
        <ChevronRight size={14} className="text-gray-300 group-hover:text-gray-500 transition-colors" />
      </div>
    </Link>
  )
}

export default function TrainerDashboardPage() {
  const { data: dashboard, isLoading } = useQuery({
    queryKey: ['trainer-dashboard'],
    queryFn: dashboardApi.trainer,
    refetchInterval: 60_000,
  })

  if (isLoading) return (
    <div className="flex items-center justify-center h-64">
      <div className="w-6 h-6 border-2 border-indigo-600 border-t-transparent rounded-full animate-spin" />
    </div>
  )

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-xl font-semibold text-gray-900">Visão Geral</h1>
        <p className="text-sm text-gray-500 mt-0.5">Acompanhe seus alunos e treinos</p>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard label="Total de alunos" value={dashboard?.totalStudents ?? 0}
          icon={Users} color="bg-indigo-500" />
        <StatCard label="Ativos esta semana" value={dashboard?.activeStudentsThisWeek ?? 0}
          icon={Activity} color="bg-green-500" />
        <StatCard label="Treinos prescritos" value={dashboard?.totalPlans ?? 0}
          icon={Dumbbell} color="bg-amber-500" />
        <StatCard label="Taxa de adesão" value={
          dashboard && dashboard.totalStudents > 0
            ? `${Math.round((dashboard.activeStudentsThisWeek / dashboard.totalStudents) * 100)}%`
            : '—'
        } icon={TrendingUp} color="bg-purple-500" />
      </div>

      {/* Student Activity */}
      <div className="bg-white rounded-xl border border-gray-100">
        <div className="flex items-center justify-between px-4 py-4 border-b border-gray-50">
          <h2 className="text-sm font-semibold text-gray-900">Atividade dos Alunos</h2>
          <Link href="/students" className="text-xs text-indigo-600 hover:text-indigo-700 font-medium">
            Ver todos
          </Link>
        </div>
        <div className="divide-y divide-gray-50">
          {dashboard?.studentActivity.slice(0, 8).map(a => (
            <StudentRow key={a.studentId} activity={a} />
          ))}
          {(!dashboard?.studentActivity.length) && (
            <div className="py-12 text-center">
              <Users size={32} className="text-gray-200 mx-auto mb-2" />
              <p className="text-sm text-gray-400">Nenhum aluno cadastrado ainda.</p>
              <Link href="/students/new" className="text-sm text-indigo-600 hover:underline mt-1 inline-block">
                Cadastrar primeiro aluno
              </Link>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
