import * as React from 'react'
import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'
import { Box, Card, CardContent, Grid, MenuItem, TextField, Typography, Button } from '@mui/material'
import { Link as RouterLink } from 'react-router-dom'

type PedidoItemVm = { id: number; produtoId: number; nomeProduto: string; quantidade: number; valorUnitario: number; subtotal: number }
type PedidoVm = {
  id: number; pessoaId: number; dataVenda: string;
  formaPagamento: string; status: string; itens: PedidoItemVm[]; valorTotal: number
}

export default function OrdersListPage() {
  const [status, setStatus] = React.useState<string>('')
  const [pessoaId, setPessoaId] = React.useState<string>('')

  const list = useQuery({
    queryKey: ['pedidos', status, pessoaId],
    queryFn: async () => {
      const params: any = {}
      if (status) params.status = status
      if (pessoaId) params.pessoaId = Number(pessoaId)
      const r = await api.get<PedidoVm[]>('/api/pedidos', { params })
      return r.data
    }
  })

  return (
    <Box>
      <Typography variant="h5" sx={{ mb: 2 }}>Pedidos</Typography>

      <Grid container spacing={2} sx={{ mb: 2 }}>
        <Grid item xs={12} sm={3}>
          <TextField select fullWidth label="Status" value={status} onChange={e=>setStatus(e.target.value)}>
            <MenuItem value="">Todos</MenuItem>
            <MenuItem value="Pendente">Pendente</MenuItem>
            <MenuItem value="Pago">Pago</MenuItem>
            <MenuItem value="Enviado">Enviado</MenuItem>
            <MenuItem value="Recebido">Recebido</MenuItem>
          </TextField>
        </Grid>
        <Grid item xs={12} sm={3}>
          <TextField fullWidth label="PessoaId" value={pessoaId} onChange={e=>setPessoaId(e.target.value)} />
        </Grid>
        <Grid item xs={12} sm={6} display="flex" alignItems="center" gap={1}>
          <Button component={RouterLink} to="/pedidos/novo" variant="contained">Novo Pedido</Button>
          <Button onClick={()=>{ setStatus(''); setPessoaId('') }}>Limpar</Button>
        </Grid>
      </Grid>

      {list.data?.map(p => (
        <Card key={p.id} sx={{ mb: 1 }}>
          <CardContent>
            <Grid container>
              <Grid item xs={12} sm={8}>
                <Typography variant="subtitle1">Pedido #{p.id} — Pessoa {p.pessoaId}</Typography>
                <Typography variant="body2" color="text.secondary">
                  {new Date(p.dataVenda).toLocaleString()} • {p.formaPagamento} • {p.status}
                </Typography>
              </Grid>
              <Grid item xs={12} sm={4} textAlign="right">
                <Typography variant="subtitle1">Total: R$ {p.valorTotal.toFixed(2)}</Typography>
              </Grid>
            </Grid>
            <ul>
              {p.itens.map(i => (
                <li key={i.id}>{i.nomeProduto} — x{i.quantidade} • R$ {i.valorUnitario.toFixed(2)} = R$ {i.subtotal.toFixed(2)}</li>
              ))}
            </ul>
          </CardContent>
        </Card>
      ))}
    </Box>
  )
}
