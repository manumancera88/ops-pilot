import { HttpInterceptorFn } from "@angular/common/http";
import { inject } from "@angular/core";
import { Auth } from "../services/auth";

const API_ORIGIN = "http://localhost:5265";

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = inject(Auth).getToken();

  // Solo adjuntamos el token a peticiones hacia nuestra propia API
  if (!token || !req.url.startsWith(API_ORIGIN)) {
    return next(req);
  }

  return next(req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }));
};
