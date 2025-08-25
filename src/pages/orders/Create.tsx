import * as React from 'react'
import { useForm, Controller } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'
import {
  Box, Button, Card, CardContent, Grid, MenuItem, Stack, TextField, Typography, IconButton
} from '@mui/material'
import DeleteIcon from '@mui/icons-material/Delete'
import AddIcon from '@mui/icons-material/Add'

type Pessoa = { id: number; nome: string; cpf: string }
type Produto = { id: number; nome: string; valor: number; codigo: string }

const schema = z.object({
  pessoaId: z.coerce.number().gt(0),
  formaPagamento: z.string().min(1),
  produtoId: z.coerce.number().optional(),
  quantidade: z.coerce.number().optional(),
  itens: z.array(z.object({
    produtoId: z.number(),
    nome: z.string(),
    quantidade: z.number().gt(0),
    valorUnitario: z.number().nonnegative()
  })).min(1)
})

export default function OrderCreatePage() {
  const pessoas = useQuery({
    queryKey: ['pessoas', {}],
    queryFn: async () => (await api.get<Pessoa[]>('/api/pessoas')).data
  })

  const [buscaProduto, setBuscaProduto] = React.useState('')
  const produtos = useQuery({
    queryKey: ['produtos', { nome: buscaProduto }],
    queryFn: async () => (await api.get<Produto[]>('/api/produtos', { params: { nome: buscaProduto } })).data
  })

  const form = useForm<z.infer<typeof schema>>({
    resolver: zodResolver(schema),
    defaultValues: { pessoaId: 0, formaPagamento: 'Dinheiro', itens: [] }
  })

  const addItem = () => {
    const id = Number(form.getValues('produtoId') || 0)
    const qtd = Number(form.getValues('quantidade') || 0)
    if (!id || !qtd) return
    const prod = produtos.data?.find(p => p.id === id)
    if (!prod) return
    const current = form.getValues('itens')
    const exists = current.find(i => i.produtoId === id)
    if (exists) {
      exists.quantidade += qtd
      form.setValue('itens', [...current])
    } else {
      form.setValue('itens', [...current, { produtoId: id, nome: prod.nome, quantidade: qtd, valorUnitario: prod.valor }])
    }
    form.setValue('produtoId', undefined as any)
    form.setValue('quantidade', undefined as any)
  }

  const removeItem = (produtoId: number) => {
    form.setValue('itens', form.getValues('itens').filter(i => i.produtoId != produtoId))
  }

  const total = form.watch('itens').reduce((acc, it) => acc + it.quantidade * it.valorUnitario, 0)

  const createMut = useMutation({
    mutationFn: async (payload: any) => {
      const body = {
        pessoaId: payload.pessoaId,
        formaPagamento: payload.formaPagamento,
        itens: payload.itens.map((i: any) => ({ produtoId: i.produtoId, quantidade: i.quantidade }))
      }
      return (await api.post('/api/pedidos', body)).data
    }
  })

  const onSubmit = form.handleSubmit(async (d) => {
    const created = await createMut.mutateAsync(d)
    alert('Pedido criado com sucesso! ID: ' + created.id)
    form.reset({ pessoaId: 0, formaPagamento: 'Dinheiro', itens: [] })
  })

  return (
    <Box>
      <Typography variant="h5" sx={{ mb: 2 }}>Novo Pedido</Typography>

      <Card sx={{ mb: 2 }}>
        <CardContent>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <TextField select fullWidth label="Pessoa" value={form.watch('pessoaId')}
                onChange={(e)=>form.setValue('pessoaId', Number(e.target.value))}>
                <MenuItem value={0} disabled>Selecione…</MenuItem>
                {pessoas.data?.map(p => (
                  <MenuItem key={p.id} value={p.id}>{p.nome} — {p.cpf}</MenuItem>
                ))}
              </TextField>
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField select fullWidth label="Forma de Pagamento" value={form.watch('formaPagamento')}
                onChange={(e)=>form.setValue('formaPagamento', e.target.value)}>
                <MenuItem value="Dinheiro">Dinheiro</MenuItem>
                <MenuItem value="Cartao">Cartão</MenuItem>
                <MenuItem value="Boleto">Boleto</MenuItem>
              </TextField>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      <Card sx={{ mb: 2 }}>
        <CardContent>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <TextField fullWidth label="Buscar produto por nome" value={buscaProduto} onChange={e=>setBuscaProduto(e.target.value)} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField select fullWidth label="Produto" value={form.watch('produtoId') ?? ''}
                onChange={(e)=>form.setValue('produtoId', Number(e.target.value))}>
                <MenuItem value="" disabled>Selecione…</MenuItem>
                {produtos.data?.map(p => (
                  <MenuItem key={p.id} value={p.id}>{p.nome} — R$ {p.valor.toFixed(2)}</MenuItem>
                ))}
              </TextField>
            </Grid>
            <Grid item xs={8} sm={4}>
              <TextField fullWidth type="number" label="Quantidade" value={form.watch('quantidade') ?? ''}
                onChange={(e)=>form.setValue('quantidade', Number(e.target.value))} />
            </Grid>
            <Grid item xs={4} sm="auto" display="flex" alignItems="center">
              <Button variant="outlined" startIcon={<AddIcon />} onClick={addItem}>Adicionar</Button>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      <Card>
        <CardContent>
          <Typography variant="h6" sx={{ mb: 1 }}>Itens</Typography>
          <Stack gap={1}>
            {form.watch('itens').map(it => (
              <Grid key={it.produtoId} container alignItems="center">
                <Grid item xs={12} sm={6}>
                  <Typography>{it.nome}</Typography>
                  <Typography variant="body2" color="text.secondary">
                    x{it.quantidade} • R$ {it.valorUnitario.toFixed(2)} = R$ {(it.quantidade * it.valorUnitario).toFixed(2)}
                  </Typography>
                </Grid>
                <Grid item xs={12} sm={6} textAlign="right">
                  <IconButton color="error" onClick={()=>removeItem(it.produtoId)}><DeleteIcon /></IconButton>
                </Grid>
              </Grid>
            ))}
          </Stack>
          <Typography variant="h6" sx={{ mt: 2 }}>Total: R$ {total.toFixed(2)}</Typography>
          <Box sx={{ mt: 2 }}>
            <Button variant="contained" onClick={onSubmit}>Finalizar Pedido</Button>
          </Box>
        </CardContent>
      </Card>
    </Box>
  )
}
