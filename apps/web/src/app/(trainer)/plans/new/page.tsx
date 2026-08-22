'use client'
import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useForm, useFieldArray, Controller } from 'react-hook-form'
import { plansApi, studentsApi, exercisesApi, type Exercise } from '@/lib/api'
import { Plus, Trash2, GripVertical, ChevronDown, Save, ArrowLeft } from 'lucide-react'
import { useRouter } from 'next/navigation'

const DAYS = ['Segunda', 'Terça', 'Quarta', 'Quinta', 'Sexta', 'Sábado', 'Domingo']
const MUSCLE_GROUPS = ['Chest','Back','Shoulders','Biceps','Triceps','Legs','Glutes','Core','FullBody','Cardio']

type FormData = {
  studentId: string
  name: string
  description: string
  startDate: string
  endDate: string
  days: {
    dayOfWeek: number
    label: string
    notes: string
    exercises: {
      exerciseId: string
      exerciseName: string
      sets: number
      reps: string
      weightKg: number
      restSeconds: number
      coachNotes: string
    }[]
  }[]
}

function ExercisePickerModal({ open, onClose, onPick }: {
  open: boolean; onClose: () => void; onPick: (ex: Exercise) => void
}) {
  const [muscle, setMuscle] = useState('')
  const [search, setSearch] = useState('')
  const { data: exercises = [] } = useQuery({
    queryKey: ['exercises', muscle],
    queryFn: () => exercisesApi.list(muscle ? { muscle } : undefined),
    enabled: open,
  })

  const filtered = exercises.filter(e =>
    e.name.toLowerCase().includes(search.toLowerCase())
  )

  if (!open) return null
  return (
    <div style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.4)', zIndex: 50, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
      <div className="bg-white rounded-2xl w-full max-w-md mx-4 overflow-hidden">
        <div className="px-5 py-4 border-b border-gray-100 flex items-center justify-between">
          <h3 className="font-semibold text-gray-900">Adicionar exercício</h3>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600 text-lg">×</button>
        </div>
        <div className="p-4 space-y-3">
          <input placeholder="Buscar exercício..." value={search}
            onChange={e => setSearch(e.target.value)}
            className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 outline-none focus:border-indigo-400" />
          <select value={muscle} onChange={e => setMuscle(e.target.value)}
            className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 outline-none focus:border-indigo-400">
            <option value="">Todos os músculos</option>
            {MUSCLE_GROUPS.map(m => <option key={m} value={m}>{m}</option>)}
          </select>
        </div>
        <div className="overflow-y-auto max-h-64 px-4 pb-4 space-y-1">
          {filtered.map(ex => (
            <button key={ex.id} onClick={() => { onPick(ex); onClose() }}
              className="w-full text-left px-3 py-2.5 rounded-lg hover:bg-indigo-50 transition-colors group">
              <span className="text-sm font-medium text-gray-900 group-hover:text-indigo-700">{ex.name}</span>
              <span className="text-xs text-gray-400 ml-2">{ex.muscleGroup} · {ex.equipment}</span>
            </button>
          ))}
          {filtered.length === 0 && (
            <p className="text-sm text-gray-400 text-center py-6">Nenhum exercício encontrado</p>
          )}
        </div>
      </div>
    </div>
  )
}

export default function NewWorkoutPlanPage() {
  const router = useRouter()
  const qc = useQueryClient()
  const [pickerState, setPickerState] = useState<{ open: boolean; dayIndex: number }>({ open: false, dayIndex: 0 })
  const [activeDayIndex, setActiveDayIndex] = useState(0)

  const { data: students = [] } = useQuery({ queryKey: ['students'], queryFn: studentsApi.list })

  const { register, control, handleSubmit, watch, formState: { errors } } = useForm<FormData>({
    defaultValues: {
      studentId: '', name: '', description: '', startDate: '', endDate: '',
      days: [{ dayOfWeek: 0, label: 'Treino A', notes: '', exercises: [] }]
    }
  })

  const { fields: dayFields, append: appendDay, remove: removeDay } = useFieldArray({ control, name: 'days' })

  const mutation = useMutation({
    mutationFn: (data: FormData) => plansApi.create({
      studentId: data.studentId,
      name: data.name,
      description: data.description || undefined,
      startDate: data.startDate || undefined,
      endDate: data.endDate || undefined,
      days: data.days.map((d, i) => ({
        dayOfWeek: Number(d.dayOfWeek),
        label: d.label,
        notes: d.notes || undefined,
        orderIndex: i,
        exercises: d.exercises.map((e, j) => ({
          exerciseId: e.exerciseId,
          sets: Number(e.sets),
          reps: e.reps,
          weightKg: e.weightKg ? Number(e.weightKg) : undefined,
          restSeconds: Number(e.restSeconds),
          orderIndex: j,
          coachNotes: e.coachNotes || undefined,
        }))
      }))
    }),
    onSuccess: (plan) => {
      qc.invalidateQueries({ queryKey: ['plans'] })
      router.push(`/plans/${plan.id}`)
    }
  })

  const onAddExercise = (ex: Exercise) => {
    const dayPath = `days.${pickerState.dayIndex}.exercises` as const
    // Use react-hook-form setValue to append
    const currentDay = watch(`days.${pickerState.dayIndex}`)
    currentDay.exercises.push({
      exerciseId: ex.id,
      exerciseName: ex.name,
      sets: 3, reps: '12', weightKg: 0, restSeconds: 90, coachNotes: ''
    })
  }

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <div className="flex items-center gap-3">
        <button onClick={() => router.back()} className="text-gray-400 hover:text-gray-600">
          <ArrowLeft size={20} />
        </button>
        <div>
          <h1 className="text-xl font-semibold text-gray-900">Novo Plano de Treino</h1>
          <p className="text-sm text-gray-500">Prescreva o treino para um aluno</p>
        </div>
      </div>

      <form onSubmit={handleSubmit(d => mutation.mutate(d))} className="space-y-6">

        {/* Plan info */}
        <div className="bg-white rounded-xl border border-gray-100 p-5 space-y-4">
          <h2 className="text-sm font-semibold text-gray-700">Informações do plano</h2>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1">Aluno *</label>
              <select {...register('studentId', { required: true })}
                className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 outline-none focus:border-indigo-400">
                <option value="">Selecionar aluno...</option>
                {students.map(s => <option key={s.id} value={s.id}>{s.user.name}</option>)}
              </select>
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1">Nome do plano *</label>
              <input {...register('name', { required: true })} placeholder="Ex: Hipertrofia 3x"
                className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 outline-none focus:border-indigo-400" />
            </div>
          </div>

          <div>
            <label className="block text-xs font-medium text-gray-600 mb-1">Descrição</label>
            <textarea {...register('description')} rows={2} placeholder="Objetivo, observações gerais..."
              className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 outline-none focus:border-indigo-400 resize-none" />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1">Início</label>
              <input type="date" {...register('startDate')}
                className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 outline-none focus:border-indigo-400" />
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1">Término</label>
              <input type="date" {...register('endDate')}
                className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 outline-none focus:border-indigo-400" />
            </div>
          </div>
        </div>

        {/* Days */}
        <div className="bg-white rounded-xl border border-gray-100 overflow-hidden">
          {/* Day tabs */}
          <div className="flex overflow-x-auto border-b border-gray-100 bg-gray-50">
            {dayFields.map((day, i) => (
              <button key={day.id} type="button" onClick={() => setActiveDayIndex(i)}
                className={`flex-shrink-0 px-4 py-3 text-sm font-medium transition-colors border-b-2 ${
                  activeDayIndex === i
                    ? 'border-indigo-600 text-indigo-600 bg-white'
                    : 'border-transparent text-gray-500 hover:text-gray-700'
                }`}>
                {watch(`days.${i}.label`) || `Dia ${i + 1}`}
              </button>
            ))}
            <button type="button"
              onClick={() => appendDay({ dayOfWeek: dayFields.length % 7, label: `Treino ${String.fromCharCode(65 + dayFields.length)}`, notes: '', exercises: [] })}
              className="flex-shrink-0 px-4 py-3 text-sm text-indigo-600 hover:bg-indigo-50 transition-colors flex items-center gap-1">
              <Plus size={14} /> Dia
            </button>
          </div>

          {/* Active day editor */}
          {dayFields.map((day, dayIndex) => (
            <div key={day.id} className={dayIndex === activeDayIndex ? 'block' : 'hidden'}>
              <div className="p-5 space-y-4 border-b border-gray-50">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-xs font-medium text-gray-600 mb-1">Rótulo do dia</label>
                    <input {...register(`days.${dayIndex}.label`)} placeholder="Treino A"
                      className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 outline-none focus:border-indigo-400" />
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-gray-600 mb-1">Dia da semana</label>
                    <select {...register(`days.${dayIndex}.dayOfWeek`)}
                      className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 outline-none focus:border-indigo-400">
                      {DAYS.map((d, i) => <option key={i} value={i}>{d}</option>)}
                    </select>
                  </div>
                </div>
              </div>

              {/* Exercises */}
              <div className="p-5 space-y-3">
                <div className="flex items-center justify-between">
                  <h3 className="text-sm font-semibold text-gray-700">Exercícios</h3>
                  <button type="button"
                    onClick={() => setPickerState({ open: true, dayIndex })}
                    className="flex items-center gap-1.5 text-xs font-medium text-indigo-600 hover:text-indigo-700 border border-indigo-200 rounded-lg px-3 py-1.5 hover:bg-indigo-50 transition-colors">
                    <Plus size={13} /> Adicionar
                  </button>
                </div>

                <Controller control={control} name={`days.${dayIndex}.exercises`}
                  render={({ field }) => (
                    <div className="space-y-2">
                      {field.value.map((ex, exIndex) => (
                        <div key={exIndex} className="border border-gray-100 rounded-xl p-4 space-y-3">
                          <div className="flex items-center justify-between">
                            <div className="flex items-center gap-2">
                              <GripVertical size={14} className="text-gray-300" />
                              <span className="text-sm font-semibold text-gray-900">{ex.exerciseName}</span>
                            </div>
                            <button type="button"
                              onClick={() => {
                                const updated = [...field.value]
                                updated.splice(exIndex, 1)
                                field.onChange(updated)
                              }}
                              className="text-gray-300 hover:text-red-500 transition-colors">
                              <Trash2 size={14} />
                            </button>
                          </div>
                          <div className="grid grid-cols-4 gap-2">
                            {[
                              { label: 'Séries', key: 'sets', type: 'number', placeholder: '3' },
                              { label: 'Reps', key: 'reps', type: 'text', placeholder: '12' },
                              { label: 'Carga (kg)', key: 'weightKg', type: 'number', placeholder: '0' },
                              { label: 'Descanso (s)', key: 'restSeconds', type: 'number', placeholder: '90' },
                            ].map(({ label, key, type, placeholder }) => (
                              <div key={key}>
                                <label className="block text-xs text-gray-400 mb-1">{label}</label>
                                <input type={type} value={(ex as any)[key]} placeholder={placeholder}
                                  onChange={e => {
                                    const updated = [...field.value]
                                    ;(updated[exIndex] as any)[key] = e.target.value
                                    field.onChange(updated)
                                  }}
                                  className="w-full text-sm border border-gray-200 rounded-lg px-2 py-1.5 outline-none focus:border-indigo-400 text-center" />
                              </div>
                            ))}
                          </div>
                          <div>
                            <label className="block text-xs text-gray-400 mb-1">Observação do professor</label>
                            <input value={ex.coachNotes}
                              onChange={e => {
                                const updated = [...field.value]
                                updated[exIndex].coachNotes = e.target.value
                                field.onChange(updated)
                              }}
                              placeholder="Ex: Foco na descida controlada..."
                              className="w-full text-sm border border-gray-200 rounded-lg px-3 py-1.5 outline-none focus:border-indigo-400" />
                          </div>
                        </div>
                      ))}
                      {field.value.length === 0 && (
                        <div className="text-center py-8 border-2 border-dashed border-gray-100 rounded-xl">
                          <p className="text-sm text-gray-400">Adicione exercícios a este dia</p>
                        </div>
                      )}
                    </div>
                  )}
                />
              </div>

              {dayFields.length > 1 && (
                <div className="px-5 pb-4">
                  <button type="button" onClick={() => { removeDay(dayIndex); setActiveDayIndex(0) }}
                    className="text-xs text-red-400 hover:text-red-600 flex items-center gap-1">
                    <Trash2 size={12} /> Remover este dia
                  </button>
                </div>
              )}
            </div>
          ))}
        </div>

        {/* Submit */}
        <div className="flex justify-end gap-3">
          <button type="button" onClick={() => router.back()}
            className="px-4 py-2 text-sm text-gray-600 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors">
            Cancelar
          </button>
          <button type="submit" disabled={mutation.isPending}
            className="flex items-center gap-2 px-5 py-2 text-sm font-medium text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 disabled:opacity-50 transition-colors">
            <Save size={15} />
            {mutation.isPending ? 'Salvando...' : 'Salvar Plano'}
          </button>
        </div>
      </form>

      <ExercisePickerModal
        open={pickerState.open}
        onClose={() => setPickerState(s => ({ ...s, open: false }))}
        onPick={onAddExercise}
      />
    </div>
  )
}
