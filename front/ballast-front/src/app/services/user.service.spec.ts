import { HttpClient } from '@angular/common/http';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { of } from 'rxjs';
import { UserService } from './user.service';
import { IAuthResponse, IRegisterRequest } from '../interfaces/user.interface';

const localStorageMock = {
  getItem: vi.fn(),
  setItem: vi.fn(),
  removeItem: vi.fn(),
  clear: vi.fn()
};

Object.defineProperty(globalThis, 'localStorage', {
  value: localStorageMock,
  configurable: true
});

describe('UserService', () => {
  let service: UserService;
  let httpClient: {
    get: ReturnType<typeof vi.fn>;
    post: ReturnType<typeof vi.fn>;
    put: ReturnType<typeof vi.fn>;
    delete: ReturnType<typeof vi.fn>;
  };

  beforeEach(() => {
    httpClient = {
      get: vi.fn(),
      post: vi.fn(),
      put: vi.fn(),
      delete: vi.fn()
    };

    service = new UserService(httpClient as unknown as HttpClient);
    localStorageMock.clear.mockReset();
    localStorageMock.getItem.mockReset();
    localStorageMock.setItem.mockReset();
    localStorageMock.removeItem.mockReset();
  });

  it('should register a user through the api', () => {
    const payload: IRegisterRequest = {
      email: 'test@example.com',
      displayName: 'Test User',
      password: 'secret'
    };

    const response: IAuthResponse = {
      token: 'abc123',
      user: {
        id: '1',
        email: 'test@example.com',
        displayName: 'Test User',
        createdDate: '2024-01-01T00:00:00.000Z'
      }
    };

    httpClient.post.mockReturnValue(of(response));

    service.register(payload).subscribe((result) => {
      expect(result).toEqual(response);
    });

    expect(httpClient.post).toHaveBeenCalledWith('https://localhost:7296/api/users/register', payload);
  });

  it('should save auth data to state and local storage', () => {
    const response: IAuthResponse = {
      token: 'auth-token',
      user: {
        id: '42',
        email: 'user@example.com',
        displayName: 'User',
        createdDate: '2024-01-03T00:00:00.000Z'
      }
    };

    service.saveAuthResponse(response, 'fallback@example.com');

    expect(service.getCurrentUser()).toEqual({
      id: '42',
      email: 'user@example.com',
      displayName: 'User',
      createdDate: '2024-01-03T00:00:00.000Z'
    });
    expect(service.getAuthToken()).toBe('auth-token');
    expect(localStorageMock.setItem).toHaveBeenCalledWith('authToken', 'auth-token');
    expect(localStorageMock.setItem).toHaveBeenCalledWith('currentUser', expect.stringContaining('user@example.com'));
  });
});
