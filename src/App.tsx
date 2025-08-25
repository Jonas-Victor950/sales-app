import { Routes, Route, Navigate } from 'react-router-dom'
import Layout from './components/Layout'
import PeoplePage from './pages/people/List'
import ProductsPage from './pages/products/List'
import OrdersListPage from './pages/orders/List'
import OrderCreatePage from './pages/orders/Create'

export default function App() {
  return (
    <Layout>
      <Routes>
        <Route path="/" element={<Navigate to="/pessoas" replace />} />
        <Route path="/pessoas" element={<PeoplePage />} />
        <Route path="/produtos" element={<ProductsPage />} />
        <Route path="/pedidos" element={<OrdersListPage />} />
        <Route path="/pedidos/novo" element={<OrderCreatePage />} />
      </Routes>
    </Layout>
  )
}
