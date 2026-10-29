import { inject, PLATFORM_ID } from "@angular/core";
import { isPlatformBrowser } from "@angular/common";
import { CanActivateFn, Router } from "@angular/router";
import { Auth } from "../services/auth";

export const authGuard: CanActivateFn = () => {
  const platformId = inject(PLATFORM_ID);

  // En el servidor (SSR) no existe localStorage: dejamos pasar
  // y la decisión real se toma en el navegador.
  if (!isPlatformBrowser(platformId)) {
    return true;
  }

  const auth = inject(Auth);
  const router = inject(Router);

  if (auth.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(["/login"]);
};
