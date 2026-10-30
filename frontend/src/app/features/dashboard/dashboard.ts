import { Component, OnInit, inject, signal } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Router } from "@angular/router";
import { FormBuilder, ReactiveFormsModule, Validators } from "@angular/forms";
import { Auth } from "../../core/services/auth";
import { Customers } from "../../core/services/customers";
import { CustomerDto } from "../../core/services/models/customer.models";

interface CompanyDto {
  id: number;
  name: string;
  taxId: string | null;
  isActive: boolean;
}

@Component({
  selector: "app-dashboard",
  imports: [ReactiveFormsModule],
  templateUrl: "./dashboard.html",
  styleUrl: "./dashboard.scss",
})
export class Dashboard implements OnInit {
  private http = inject(HttpClient);
  private auth = inject(Auth);
  private router = inject(Router);
  private customersApi = inject(Customers);
  private fb = inject(FormBuilder);

  user = this.auth.currentUser;
  company = signal<CompanyDto | null>(null);
  customers = signal<CustomerDto[]>([]);
  loadingCustomers = signal(true);
  saving = signal(false);
  error = signal<string | null>(null);
  formError = signal<string | null>(null);

  form = this.fb.group({
    name: ["", [Validators.required, Validators.maxLength(120)]],
    email: ["", [Validators.email]],
    phone: [""],
  });

  ngOnInit(): void {
    const currentUser = this.user();
    // En el servidor (SSR) no hay usuario: la carga real ocurre en el navegador.
    if (!currentUser) return;

    this.http
      .get<CompanyDto>(
        `http://localhost:5265/api/Companies/${currentUser.companyId}`,
      )
      .subscribe({
        next: (company) => this.company.set(company),
        error: () => this.error.set("Não foi possível carregar a empresa."),
      });

    this.customersApi.getAll().subscribe({
      next: (list) => {
        this.customers.set(list);
        this.loadingCustomers.set(false);
      },
      error: () => {
        this.error.set("Não foi possível carregar os clientes.");
        this.loadingCustomers.set(false);
      },
    });
  }

  addCustomer(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.saving.set(true);
    this.formError.set(null);

    this.customersApi
      .create({
        name: (value.name ?? "").trim(),
        email: value.email?.trim() || null,
        phone: value.phone?.trim() || null,
      })
      .subscribe({
        next: (created) => {
          this.customers.update((list) => [created, ...list]);
          this.form.reset({ name: "", email: "", phone: "" });
          this.saving.set(false);
        },
        error: () => {
          this.formError.set("Não foi possível criar o cliente.");
          this.saving.set(false);
        },
      });
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(["/login"]);
  }
}
