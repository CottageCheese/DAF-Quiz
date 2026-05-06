export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  displayName: string;
}

export interface AuthUser {
  email: string;
  displayName: string;
}

export interface JwtPayload {
  sub: string;
  email: string;
  display_name?: string;
  exp: number;
  role?: string | string[];
}
