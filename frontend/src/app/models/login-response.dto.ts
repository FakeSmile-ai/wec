export interface RoleDto {
  id: number;
  name: string;
}

export interface LoginResponseDto {
  token: string;
  username: string;
  role: RoleDto;
}
