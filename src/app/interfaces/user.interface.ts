export interface IUserDto {
  id: string;
  email: string;
  displayName: string;
  createdDate: string;
}

export interface IRegisterRequest {
  email: string;
  displayName: string;
  password: string;
}

export interface ILoginRequest {
  email: string;
  password: string;
}

export interface IAuthResponse {
  user?: IUserDto;
  token: string;
  Id?: string;
  id?: string;
  email?: string;
  displayName?: string;
}
