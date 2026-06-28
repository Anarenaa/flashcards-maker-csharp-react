import '@testing-library/jest-dom';
import { beforeAll, afterEach, afterAll } from 'vitest';
import { setupServer } from 'msw/node';

export const server = setupServer();

beforeAll(() => server.listen());
afterEach(() => server.resetHandlers()); // Очищає фейкові відповіді після кожного тесту
afterAll(() => server.close());