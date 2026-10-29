import { Component, OnInit, inject, signal } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Router } from "@angular/router";
import { Auth } from "../../core/services/auth";

interface CompanyDto {
  id: number;
  name: string;
  taxId: string | null;
  isActive: boolean;
}

@Component({
  selector: "app-dashboard",
  imports: [],
  templateUrl: "./dashboard.html",
  styleUrl: "./dashboard.scss",
})
export class Dashboard implements OnInit {
  private http = inject(HttpClient);
  private auth = inject(Auth);
  private router = inject(Router);

  user = this.auth.currentUser;
  company = signal<CompanyDto | null>(null);
  error = signal<string | null>(null);

  ngOnInit(): void {
    const currentUser = this.user();
    if (!currentUser) return;

    this.http
      .get<CompanyDto>(
        `http://localhost:5265/api/Companies/${currentUser.companyId}`,
      )
      .subscribe({
        next: (company) => this.company.set(company),
        error: () => this.error.set("Não foi possível carregar a empresa."),
      });
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(["/login"]);
  }
}
