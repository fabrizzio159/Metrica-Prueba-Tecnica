import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Box, Typography, Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
  Paper, Chip, IconButton, CircularProgress, Alert } from '@mui/material';
import RefreshIcon from '@mui/icons-material/Refresh';
import VisibilityIcon from '@mui/icons-material/Visibility';
import { cargaService } from '../services/api';

const estadoColor = {
  Pendiente: 'warning',
  EnProceso: 'info',
  Cargado: 'primary',
  Finalizado: 'success',
  Notificado: 'success',
  Error: 'error',
  ConErrores: 'error',
};

export default function Historial() {
  const navigate = useNavigate();
  const [cargas, setCargas] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const fetchHistorial = async () => {
    setLoading(true);
    setError('');
    try {
      const res = await cargaService.getHistorial();
      setCargas(res.data);
    } catch (err) {
      setError('Error al cargar historial');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchHistorial(); }, []);

  if (loading) return <Box display="flex" justifyContent="center" mt={4}><CircularProgress /></Box>;

  return (
    <Box>
      <Box display="flex" alignItems="center" mb={2}>
        <Typography variant="h5" sx={{ flexGrow: 1 }}>Historial de Cargas</Typography>
        <IconButton onClick={fetchHistorial}><RefreshIcon /></IconButton>
      </Box>
      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
      {cargas.length === 0 ? (
        <Typography color="text.secondary">No hay cargas registradas.</Typography>
      ) : (
        <TableContainer component={Paper}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>ID</TableCell>
                <TableCell>Archivo</TableCell>
                <TableCell>Periodo</TableCell>
                <TableCell>Estado</TableCell>
                <TableCell>Filas</TableCell>
                <TableCell>Fecha Registro</TableCell>
                <TableCell>Fecha Fin</TableCell>
                <TableCell>Error</TableCell>
                <TableCell>Acciones</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {cargas.map((c) => (
                <TableRow key={c.id}>
                  <TableCell>{c.id}</TableCell>
                  <TableCell>{c.nombreArchivo}</TableCell>
                  <TableCell>{c.periodo || '-'}</TableCell>
                  <TableCell>
                    <Chip label={c.estado} color={estadoColor[c.estado] || 'default'} size="small" />
                  </TableCell>
                  <TableCell>{c.totalFilas}</TableCell>
                  <TableCell>{new Date(c.fechaRegistro).toLocaleString()}</TableCell>
                  <TableCell>{c.fechaFin ? new Date(c.fechaFin).toLocaleString() : '-'}</TableCell>
                  <TableCell>{c.mensajeError || '-'}</TableCell>
                  <TableCell>
                    <IconButton size="small" color="primary" onClick={() => navigate(`/detalle/${c.id}`)}>
                      <VisibilityIcon fontSize="small" />
                    </IconButton>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}
    </Box>
  );
}
