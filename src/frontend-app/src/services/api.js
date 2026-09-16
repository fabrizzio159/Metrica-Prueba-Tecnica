import axios from 'axios';

const API_BASE_URL = process.env.REACT_APP_API_URL || '';

const api = axios.create({ baseURL: API_BASE_URL });

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

api.interceptors.response.use(
  (res) => res,
  async (error) => {
    const orig = error.config;
    if (error.response?.status === 401 && !orig._retry) {
      orig._retry = true;
      const rt = localStorage.getItem('refreshToken');
      if (rt) {
        try {
          const res = await axios.post(`${API_BASE_URL}/api/auth/refresh`, { refreshToken: rt });
          localStorage.setItem('token', res.data.token);
          localStorage.setItem('refreshToken', res.data.refreshToken);
          orig.headers.Authorization = `Bearer ${res.data.token}`;
          return api(orig);
        } catch {
          localStorage.clear();
          window.location.href = '/login';
        }
      }
    }
    return Promise.reject(error);
  }
);

export const authService = {
  login: (email, password) => api.post('/api/auth/login', { email, password }),
};

export const cargaService = {
  upload: (file) => {
    const fd = new FormData();
    fd.append('file', file);
    return api.post('/api/carga/upload', fd, { headers: { 'Content-Type': 'multipart/form-data' } });
  },
  getHistorial: () => api.get('/api/carga/historial'),
  getDetalle: (id) => api.get(`/api/carga/${id}/detalle`),
  reemplazar: (detalleId) => api.post(`/api/carga/detalle/${detalleId}/reemplazar`),
};

export default api;
