import api from './api';
import toast from 'react-hot-toast';

vi.mock('react-hot-toast', () => ({
  default: {
    error: vi.fn(),
  },
}));

describe('API Interceptor Global Errors & Edge Cases', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.stubGlobal('window', {
      location: { href: '' },
      navigator: { onLine: true },
    });
  });

  const getRejectedHandler = () => api.interceptors.response.handlers[0].rejected;

  it('handles network error (offline or no connection)', async () => {
    const error = { message: 'Network Error', config: {} };
    const errorHandler = getRejectedHandler();

    await expect(errorHandler(error)).rejects.toEqual(error);
    expect(toast.error).toHaveBeenCalledWith(
      "Здається, у вас зник інтернет. Перевірте підключення до мережі 🌐",
      expect.any(Object)
    );
  });

  it('handles 429 Too Many Requests', async () => {
    const error = {
      response: { status: 429 },
      config: {},
    };
    const errorHandler = getRejectedHandler();

    await expect(errorHandler(error)).rejects.toEqual(error);
    expect(toast.error).toHaveBeenCalledWith(
      "Занадто багато запитів! Будь ласка, спробуйте трохи пізніше.",
      expect.any(Object)
    );
  });

  it('handles 500 server error', async () => {
    const error = {
      response: { status: 500 },
      config: {},
    };
    const errorHandler = getRejectedHandler();

    await expect(errorHandler(error)).rejects.toEqual(error);
    expect(toast.error).toHaveBeenCalledWith(
      "Не вдалося з'єднатися з сервером. Спробуйте пізніше.",
      expect.any(Object)
    );
  });

  it('ignores cancelled requests without showing a toast', async () => {
    const error = { code: 'ERR_CANCELED', config: {} };
    const errorHandler = getRejectedHandler();

    await expect(errorHandler(error)).rejects.toEqual(error);
    expect(toast.error).not.toHaveBeenCalled();
  });
});