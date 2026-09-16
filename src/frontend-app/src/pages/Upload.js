import { useState } from 'react';
import { Box, Button, Typography, Alert, Paper, LinearProgress } from '@mui/material';
import CloudUploadIcon from '@mui/icons-material/CloudUpload';
import { cargaService } from '../services/api';

export default function Upload() {
  const [file, setFile] = useState(null);
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState(null);
  const [error, setError] = useState('');

  const handleFileChange = (e) => {
    const selected = e.target.files[0];
    if (selected && !selected.name.endsWith('.xlsx')) {
      setError('Solo se permiten archivos .xlsx');
      setFile(null);
      return;
    }
    setError('');
    setResult(null);
    setFile(selected);
  };

  const handleUpload = async () => {
    if (!file) return;
    setLoading(true);
    setError('');
    setResult(null);
    try {
      const res = await cargaService.upload(file);
      setResult(res.data);
      setFile(null);
    } catch (err) {
      setError(err.response?.data?.message || 'Error al subir el archivo');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box>
      <Typography variant="h5" gutterBottom>Subir Archivo Excel</Typography>
      <Paper sx={{ p: 4, textAlign: 'center' }}>
        <input type="file" accept=".xlsx" onChange={handleFileChange}
          style={{ display: 'none' }} id="file-input" />
        <label htmlFor="file-input">
          <Button variant="outlined" component="span" startIcon={<CloudUploadIcon />}>
            Seleccionar Archivo
          </Button>
        </label>
        {file && (
          <Typography sx={{ mt: 2 }} color="text.secondary">
            {file.name} ({(file.size / 1024).toFixed(1)} KB)
          </Typography>
        )}
        {loading && <LinearProgress sx={{ mt: 2 }} />}
        {error && <Alert severity="error" sx={{ mt: 2 }}>{error}</Alert>}
        {result && (
          <Alert severity="success" sx={{ mt: 2 }}>
            Archivo subido. ID: {result.idCarga} — Estado: {result.estado}
          </Alert>
        )}
        <Box mt={3}>
          <Button variant="contained" onClick={handleUpload}
            disabled={!file || loading}>
            {loading ? 'Subiendo...' : 'Subir'}
          </Button>
        </Box>
      </Paper>
    </Box>
  );
}
