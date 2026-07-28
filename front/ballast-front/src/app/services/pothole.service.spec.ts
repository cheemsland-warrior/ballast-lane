import { HttpClient } from '@angular/common/http';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { of } from 'rxjs';
import { PotholeService } from './pothole.service';
import { IPotholeCreateDto, IPotholeDto } from '../interfaces/pothole.interface';

describe('PotholeService', () => {
  let service: PotholeService;
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

    service = new PotholeService(httpClient as unknown as HttpClient);
  });

  it('should fetch all potholes', () => {
    const expected: IPotholeDto[] = [
      {
        id: '1',
        description: 'Large crack',
        latitude: 10.1,
        longitude: 20.2,
        status: 'open',
        createdDate: '2024-01-01T00:00:00.000Z',
        userId: 'user-1'
      }
    ];

    httpClient.get.mockReturnValue(of(expected));

    service.getAll().subscribe((response) => {
      expect(response).toEqual(expected);
    });

    expect(httpClient.get).toHaveBeenCalledWith('https://localhost:7296/api/potholes');
  });

  it('should create a pothole', () => {
    const payload: IPotholeCreateDto = {
      description: 'New pothole',
      latitude: 5,
      longitude: 6,
      status: 'open',
      userId: 'user-1'
    };

    const response: IPotholeDto = {
      id: '2',
      description: 'New pothole',
      latitude: 5,
      longitude: 6,
      status: 'open',
      createdDate: '2024-01-02T00:00:00.000Z',
      userId: 'user-1'
    };

    httpClient.post.mockReturnValue(of(response));

    service.create(payload).subscribe((result) => {
      expect(result).toEqual(response);
    });

    expect(httpClient.post).toHaveBeenCalledWith('https://localhost:7296/api/potholes', payload);
  });
});
