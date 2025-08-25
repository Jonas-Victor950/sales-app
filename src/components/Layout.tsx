import * as React from 'react'
import { AppBar, Toolbar, Typography, Container, Button, Box } from '@mui/material'
import { Link as RouterLink, useLocation } from 'react-router-dom'

export default function Layout({ children }: { children: React.ReactNode }) {
  const { pathname } = useLocation()
  const nav = [
    { to: '/pessoas', label: 'Pessoas' },
    { to: '/produtos', label: 'Produtos' },
    { to: '/pedidos', label: 'Pedidos' },
    { to: '/pedidos/novo', label: 'Novo Pedido' },
  ]

  return (
    <Box>
      <AppBar position="static">
        <Toolbar>
          <Typography variant="h6" sx={{ flexGrow: 1 }}>SalesApp</Typography>
          {nav.map(n => (
            <Button
              key={n.to}
              color={pathname.startsWith(n.to) ? 'secondary' : 'inherit'}
              component={RouterLink}
              to={n.to}
              sx={{ ml: 1 }}
            >
              {n.label}
            </Button>
          ))}
        </Toolbar>
      </AppBar>
      <Container sx={{ py: 3 }}>
        {children}
      </Container>
    </Box>
  )
}
