import { Injectable, signal } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable, tap } from "rxjs";
import { LoginDto, AuthResponseDto, UserDto } from "./models/auth.models";

const API_URL = "http://localhost:5265/api";
const TOKEN_KEY = "opspilot_token";
const USER_KEY = "opspilot_user";

@Injectable({ providedIn: "root" })
export class Auth {
  currentUser = signal<UserDto | null>(this.loadUser());

  constructor(private http: HttpClient) {}

  login(credentials: LoginDto): Observable<AuthResponseDto> {
    return this.http
      .post<AuthResponseDto>(`${API_URL}/auth/login`, credentials)
      .pipe(
        tap((response) => {
          this.setSession(response.token, response.user);
          this.currentUser.set(response.user);
        }),
      );
  }

  logout(): void {
    if (typeof window !== "undefined") {
      localStorage.removeItem(TOKEN_KEY);
      localStorage.removeItem(USER_KEY);
    }
    this.currentUser.set(null);
  }

  getToken(): string | null {
    if (typeof window === "undefined") return null;
    return localStorage.getItem(TOKEN_KEY);
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  private setSession(token: string, user: UserDto): void {
    if (typeof window !== "undefined") {
      localStorage.setItem(TOKEN_KEY, token);
      localStorage.setItem(USER_KEY, JSON.stringify(user));
    }
  }

  private loadUser(): UserDto | null {
    if (typeof window === "undefined") return null;
    const raw = localStorage.getItem(USER_KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as UserDto;
    } catch {
      return null;
    }
  }
}
