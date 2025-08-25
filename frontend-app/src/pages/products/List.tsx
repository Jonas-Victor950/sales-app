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

type Produto = { id: number; nome: string; codigo: string; valor: number }

const filterSchema = z.object({
  nome: z.string().optional(),
  codigo: z.string().optional(),
  valorMin: z.coerce.number().optional(),
  valorMax: z.coerce.number().optional()
})

const formSchema = z.object({
  nome: z.string().min(1),
  codigo: z.string().min(1),
  valor: z.coerce.number().nonnegative()
})

function useListProdutos(params: any) {
  return useQuery({
    queryKey: ['produtos', params],
    queryFn: async () => {
      const r = await api.get<Produto[]>('/api/produtos', { params })
      return r.data
    }
  })
}

export default function ProductsPage() {
  const qc = useQueryClient()
  const [filters, setFilters] = React.useState<any>({})
  const { register, handleSubmit, reset } = useForm<z.infer<typeof filterSchema>>({ resolver: zodResolver(filterSchema) })
  const list = useListProdutos(filters)

  const createMut = useMutation({
    mutationFn: async (data: z.infer<typeof formSchema>) => {
      const r = await api.post('/api/produtos', data)
      return r.data as Produto
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['produtos'] })
  })

  const updateMut = useMutation({
    mutationFn: async ({ id, data }: { id: number; data: z.infer<typeof formSchema> }) => {
      const r = await api.put(`/api/produtos/${id}`, data)
      return r.data as Produto
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['produtos'] })
  })

  const deleteMut = useMutation({
    mutationFn: async (id: number) => {
      await api.delete(`/api/produtos/${id}`)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['produtos'] })
  })

  const [open, setOpen] = React.useState(false)
  const [editing, setEditing] = React.useState<Produto | null>(null)

  const form = useForm<z.infer<typeof formSchema>>({
    resolver: zodResolver(formSchema),
    defaultValues: { nome: '', codigo: '', valor: 0 }
  })

  const onSubmitFilters = (d: z.infer<typeof filterSchema>) => setFilters(d)

  const openCreate = () => { setEditing(null); form.reset({ nome: '', codigo: '', valor: 0 }); setOpen(true) }
  const openEdit = (p: Produto) => { setEditing(p); form.reset({ nome: p.nome, codigo: p.codigo, valor: p.valor }); setOpen(true) }
  const submitForm = form.handleSubmit(async (d) => {
    if (editing) await updateMut.mutateAsync({ id: editing.id, data: d })
    else await createMut.mutateAsync(d)
    setOpen(false)
  })

  return (
    <Box>
      <Typography variant="h5" sx={{ mb: 2 }}>Produtos</Typography>

      <Card sx={{ mb: 2 }}>
        <CardContent>
          <form onSubmit={handleSubmit(onSubmitFilters)}>
            <Grid container spacing={2}>
              <Grid item xs={12} sm={3}><TextField label="Nome" fullWidth {...register('nome')} /></Grid>
              <Grid item xs={12} sm={3}><TextField label="Código" fullWidth {...register('codigo')} /></Grid>
              <Grid item xs={12} sm={2}><TextField label="Valor mín." type="number" fullWidth {...register('valorMin')} /></Grid>
              <Grid item xs={12} sm={2}><TextField label="Valor máx." type="number" fullWidth {...register('valorMax')} /></Grid>
              <Grid item xs={12} sm={2} display="flex" alignItems="center" gap={1}>
                <Button type="submit" variant="contained">Filtrar</Button>
                <Button type="button" onClick={() => { reset({}); setFilters({}) }}>Limpar</Button>
                <Button type="button" variant="outlined" onClick={openCreate}>Novo Produto</Button>
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
                  <Typography variant="subtitle1">{p.nome} — R$ {p.valor.toFixed(2)}</Typography>
                  <Typography variant="body2" color="text.secondary">Código: {p.codigo}</Typography>
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
        <DialogTitle>{editing ? 'Editar Produto' : 'Novo Produto'}</DialogTitle>
        <DialogContent>
          <Box component="form" onSubmit={(e)=>{e.preventDefault()}} sx={{ mt: 1 }}>
            <Stack gap={2}>
              <TextField label="Nome" {...form.register('nome')} error={!!form.formState.errors.nome} helperText={form.formState.errors.nome?.message} />
              <TextField label="Código" {...form.register('codigo')} error={!!form.formState.errors.codigo} helperText={form.formState.errors.codigo?.message} />
              <TextField label="Valor" type="number" {...form.register('valor', { valueAsNumber: true })} error={!!form.formState.errors.valor} helperText={form.formState.errors.valor?.message} />
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
