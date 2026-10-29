export interface LoginDto {
  email: string;
  password: string;
}

export interface UserDto {
  id: number;
  name: string;
  email: string;
  isActive: boolean;
  companyId: number;
}

export interface AuthResponseDto {
  token: string;
  user: UserDto;
}
