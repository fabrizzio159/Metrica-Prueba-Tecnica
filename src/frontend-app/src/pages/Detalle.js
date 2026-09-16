import { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Box, Typography, Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
  Paper, Chip, CircularProgress, Alert, Button, Card, CardContent, Grid, Tooltip } from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import SwapHorizIcon from '@mui/icons-material/SwapHoriz';
import { cargaService } from '../services/api';

const estadoColor = {
  Pendiente: 'warning',
  EnProceso: 'info',
  Cargado: 'primary',
  Finalizado: 'success',
  Notificado: 'success',
  Error: 'error',
  ConErrores: 'error',
  Procesado: 'success',
};

export default function Detalle() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [detalle, setDetalle] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [replacing, setReplacing] = useState(null);

  const fetchDetalle = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const res = await cargaService.getDetalle(id);
      setDetalle(res.data);
    } catch (err) {
      setError('Error al cargar el detalle de la carga.');
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => { fetchDetalle(); }, [fetchDetalle]);

  const handleReemplazar = async (detalleId) => {
    setReplacing(detalleId);
    try {
      await cargaService.reemplazar(detalleId);
      await fetchDetalle();
    } catch (err) {
      setError('Error al reemplazar el registro.');
    } finally {
      setReplacing(null);
    }
  };

  if (loading) return <Box display="flex" justifyContent="center" mt={4}><CircularProgress /></Box>;

  if (error && !detalle) return (
    <Box>
      <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/historial')} sx={{ mb: 2 }}>Volver</Button>
      <Alert severity="error">{error}</Alert>
    </Box>
  );

  if (!detalle) return null;

  return (
    <Box>
      <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/historial')} sx={{ mb: 2 }}>
        Volver al Historial
      </Button>

      {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError('')}>{error}</Alert>}

      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h5" gutterBottom>Carga #{detalle.id}</Typography>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6} md={4}>
              <Typography variant="body2" color="text.secondary">Archivo</Typography>
              <Typography variant="body1">{detalle.nombreArchivo}</Typography>
            </Grid>
            <Grid item xs={12} sm={6} md={4}>
              <Typography variant="body2" color="text.secondary">Estado</Typography>
              <Chip label={detalle.estado} color={estadoColor[detalle.estado] || 'default'} size="small" />
            </Grid>
            <Grid item xs={12} sm={6} md={4}>
              <Typography variant="body2" color="text.secondary">Periodo</Typography>
              <Typography variant="body1">{detalle.periodo || '-'}</Typography>
            </Grid>
            <Grid item xs={12} sm={6} md={4}>
              <Typography variant="body2" color="text.secondary">Total Filas</Typography>
              <Typography variant="body1">{detalle.totalFilas}</Typography>
            </Grid>
            <Grid item xs={12} sm={6} md={4}>
              <Typography variant="body2" color="text.secondary">Fecha Registro</Typography>
              <Typography variant="body1">{new Date(detalle.fechaRegistro).toLocaleString()}</Typography>
            </Grid>
            <Grid item xs={12} sm={6} md={4}>
              <Typography variant="body2" color="text.secondary">Fecha Fin</Typography>
              <Typography variant="body1">{detalle.fechaFin ? new Date(detalle.fechaFin).toLocaleString() : '-'}</Typography>
            </Grid>
            {detalle.mensajeError && (
              <Grid item xs={12}>
                <Alert severity="error">{detalle.mensajeError}</Alert>
              </Grid>
            )}
          </Grid>
        </CardContent>
      </Card>

      <Typography variant="h6" gutterBottom>
        Datos Procesados ({detalle.datos.length} registros)
      </Typography>

      {detalle.datos.length === 0 ? (
        <Typography color="text.secondary">No hay datos procesados para esta carga.</Typography>
      ) : (
        <TableContainer component={Paper}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>ID</TableCell>
                <TableCell>Periodo</TableCell>
                <TableCell>Codigo Producto</TableCell>
                <TableCell>Nombre Producto</TableCell>
                <TableCell align="right">Precio</TableCell>
                <TableCell>Estado</TableCell>
                <TableCell>Fecha Registro</TableCell>
                <TableCell>Acciones</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {detalle.datos.map((d) => (
                <TableRow key={d.id} sx={d.estado === 'Error' ? { backgroundColor: 'error.light', opacity: 0.9 } : {}}>
                  <TableCell>{d.id}</TableCell>
                  <TableCell>{d.periodo}</TableCell>
                  <TableCell>{d.codigoProducto}</TableCell>
                  <TableCell>{d.nombreProducto}</TableCell>
                  <TableCell align="right">{d.precio.toFixed(2)}</TableCell>
                  <TableCell>
                    <Chip label={d.estado} color={estadoColor[d.estado] || 'default'} size="small" />
                    {d.mensajeError && (
                      <Typography variant="caption" display="block" color="error">{d.mensajeError}</Typography>
                    )}
                  </TableCell>
                  <TableCell>{new Date(d.fechaRegistro).toLocaleString()}</TableCell>
                  <TableCell>
                    {d.estado === 'Error' && (
                      <Tooltip title="Reemplazar el registro existente con este">
                        <Button
                          size="small"
                          variant="contained"
                          color="warning"
                          startIcon={replacing === d.id ? <CircularProgress size={16} /> : <SwapHorizIcon />}
                          onClick={() => handleReemplazar(d.id)}
                          disabled={replacing !== null}
                        >
                          Reemplazar
                        </Button>
                      </Tooltip>
                    )}
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
