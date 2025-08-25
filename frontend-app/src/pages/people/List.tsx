import * as React from 'react'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import {
  Box, Button, Card, CardContent, Grid, TextField, Typography, IconButton, Stack, Dialog, DialogTitle,
  DialogContent, DialogActions
} from '@mui/material'
import DeleteIcon from '@mui/icons-material/Delete'
import EditIcon from '@mui/icons-material/Edit'

type Pessoa = { id: number; nome: string; cpf: string; endereco?: string | null }

const filterSchema = z.object({
  nome: z.string().optional(),
  cpf: z.string().optional()
})

const formSchema = z.object({
  nome: z.string().min(1, 'Nome obrigatório'),
  cpf: z.string().min(11, 'CPF inválido').max(14),
  endereco: z.string().optional()
})

function useListPessoas(params: { nome?: string; cpf?: string }) {
  return useQuery({
    queryKey: ['pessoas', params],
    queryFn: async () => {
      const r = await api.get<Pessoa[]>('/api/pessoas', { params })
      return r.data
    }
  })
}

export default function PeoplePage() {
  const qc = useQueryClient()

  const [filters, setFilters] = React.useState<{ nome?: string; cpf?: string }>({})
  const { register, handleSubmit, reset } = useForm<z.infer<typeof filterSchema>>({ resolver: zodResolver(filterSchema) })
  const list = useListPessoas(filters)

  const createMut = useMutation({
    mutationFn: async (data: z.infer<typeof formSchema>) => {
      const r = await api.post('/api/pessoas', data)
      return r.data as Pessoa
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['pessoas'] })
  })

  const updateMut = useMutation({
    mutationFn: async ({ id, data }: { id: number; data: z.infer<typeof formSchema> }) => {
      const r = await api.put(`/api/pessoas/${id}`, data)
      return r.data as Pessoa
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['pessoas'] })
  })

  const deleteMut = useMutation({
    mutationFn: async (id: number) => {
      await api.delete(`/api/pessoas/${id}`)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['pessoas'] })
  })

  // modal state
  const [open, setOpen] = React.useState(false)
  const [editing, setEditing] = React.useState<Pessoa | null>(null)

  const form = useForm<z.infer<typeof formSchema>>({
    resolver: zodResolver(formSchema),
    defaultValues: { nome: '', cpf: '', endereco: '' }
  })

  const onSubmitFilters = (d: z.infer<typeof filterSchema>) => setFilters(d)

  const openCreate = () => {
    setEditing(null)
    form.reset({ nome: '', cpf: '', endereco: '' })
    setOpen(true)
  }

  const openEdit = (p: Pessoa) => {
    setEditing(p)
    form.reset({ nome: p.nome, cpf: p.cpf, endereco: p.endereco ?? '' })
    setOpen(true)
  }

  const submitForm = form.handleSubmit(async (d) => {
    if (editing) await updateMut.mutateAsync({ id: editing.id, data: d })
    else await createMut.mutateAsync(d)
    setOpen(false)
  })

  return (
    <Box>
      <Typography variant="h5" sx={{ mb: 2 }}>Pessoas</Typography>

      <Card sx={{ mb: 2 }}>
        <CardContent>
          <form onSubmit={handleSubmit(onSubmitFilters)}>
            <Grid container spacing={2}>
              <Grid item xs={12} sm={4}>
                <TextField label="Nome" fullWidth {...register('nome')} />
              </Grid>
              <Grid item xs={12} sm={4}>
                <TextField label="CPF" fullWidth {...register('cpf')} />
              </Grid>
              <Grid item xs={12} sm={4} display="flex" alignItems="center" gap={1}>
                <Button type="submit" variant="contained">Filtrar</Button>
                <Button type="button" onClick={() => { reset({}); setFilters({}) }}>Limpar</Button>
                <Button type="button" variant="outlined" onClick={openCreate}>Nova Pessoa</Button>
              </Grid>
            </Grid>
          </form>
        </CardContent>
      </Card>

      <Stack gap={1}>
        {(list.data ?? []).map(p => (
          <Card key={p.id}>
            <CardContent>
              <Grid container alignItems="center">
                <Grid item xs={10}>
                  <Typography variant="subtitle1">{p.nome}</Typography>
                  <Typography variant="body2" color="text.secondary">CPF: {p.cpf} • {p.endereco || 'Sem endereço'}</Typography>
                </Grid>
                <Grid item xs={2} textAlign="right">
                  <IconButton onClick={() => openEdit(p)}><EditIcon /></IconButton>
                  <IconButton color="error" onClick={() => deleteMut.mutate(p.id)}><DeleteIcon /></IconButton>
                </Grid>
              </Grid>
            </CardContent>
          </Card>
        ))}
      </Stack>

      <Dialog open={open} onClose={() => setOpen(false)} fullWidth maxWidth="sm">
        <DialogTitle>{editing ? 'Editar Pessoa' : 'Nova Pessoa'}</DialogTitle>
        <DialogContent>
          <Box component="form" onSubmit={(e)=>{e.preventDefault()}} sx={{ mt: 1 }}>
            <Stack gap={2}>
              <TextField label="Nome" {...form.register('nome')} error={!!form.formState.errors.nome} helperText={form.formState.errors.nome?.message} />
              <TextField label="CPF (só números)" {...form.register('cpf')} error={!!form.formState.errors.cpf} helperText={form.formState.errors.cpf?.message} />
              <TextField label="Endereço" {...form.register('endereco')} />
            </Stack>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpen(false)}>Cancelar</Button>
          <Button onClick={submitForm} variant="contained">{editing ? 'Salvar' : 'Criar'}</Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}
