import { IUserDto } from './user.interface';

export interface IPotholeDto {
  id: string;
  description: string;
  latitude: number;
  longitude: number;
  status: string;
  createdDate: string;
  userId: string;
  user?: IUserDto;
}

export interface IPotholeCreateDto {
  description: string;
  latitude: number;
  longitude: number;
  status?: string;
  userId: string;
}
