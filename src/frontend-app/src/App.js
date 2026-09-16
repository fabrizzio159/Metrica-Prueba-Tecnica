import { BrowserRouter, Routes, Route, Navigate, Link, useNavigate } from 'react-router-dom';
import { AppBar, Toolbar, Typography, Button, Container } from '@mui/material';
import Login from './pages/Login';
import Upload from './pages/Upload';
import Historial from './pages/Historial';
import Detalle from './pages/Detalle';

function isAuth() {
  return !!localStorage.getItem('token');
}

function PrivateRoute({ children }) {
  return isAuth() ? children : <Navigate to="/login" />;
}

function Layout({ children }) {
  const navigate = useNavigate();
  const handleLogout = () => {
    localStorage.clear();
    navigate('/login');
  };

  return (
    <>
      <AppBar position="static">
        <Toolbar>
          <Typography variant="h6" sx={{ flexGrow: 1 }}>Carga Masiva</Typography>
          <Button color="inherit" component={Link} to="/">Upload</Button>
          <Button color="inherit" component={Link} to="/historial">Historial</Button>
          <Button color="inherit" onClick={handleLogout}>Salir</Button>
        </Toolbar>
      </AppBar>
      <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
        {children}
      </Container>
    </>
  );
}

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route path="/" element={
          <PrivateRoute>
            <Layout><Upload /></Layout>
          </PrivateRoute>
        } />
        <Route path="/historial" element={
          <PrivateRoute>
            <Layout><Historial /></Layout>
          </PrivateRoute>
        } />
        <Route path="/detalle/:id" element={
          <PrivateRoute>
            <Layout><Detalle /></Layout>
          </PrivateRoute>
        } />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
