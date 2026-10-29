import { Injectable, signal } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable, tap } from "rxjs";
import { LoginDto, AuthResponseDto, UserDto } from "./models/auth.models";

const API_URL = "http://localhost:5265/api";
const TOKEN_KEY = "opspilot_token";

@Injectable({ providedIn: "root" })
export class Auth {
  currentUser = signal<UserDto | null>(null);

  constructor(private http: HttpClient) {}

  login(credentials: LoginDto): Observable<AuthResponseDto> {
    return this.http
      .post<AuthResponseDto>(`${API_URL}/auth/login`, credentials)
      .pipe(
        tap((response) => {
          this.setToken(response.token);
          this.currentUser.set(response.user);
        }),
      );
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    this.currentUser.set(null);
  }

  getToken(): string | null {
    if (typeof window === "undefined") return null;
    return localStorage.getItem(TOKEN_KEY);
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  private setToken(token: string): void {
    if (typeof window !== "undefined") {
      localStorage.setItem(TOKEN_KEY, token);
    }
  }
}
