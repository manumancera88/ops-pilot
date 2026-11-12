import {
  Component,
  OnInit,
  computed,
  inject,
  input,
  signal,
} from "@angular/core";
import { DatePipe } from "@angular/common";
import { FormBuilder, ReactiveFormsModule, Validators } from "@angular/forms";
import { Auth } from "../../core/services/auth";
import { Appointments } from "../../core/services/appointments";
import { AppointmentDto } from "../../core/services/models/appointment.models";
import { CustomerDto } from "../../core/services/models/customer.models";

@Component({
  selector: "app-appointment-panel",
  imports: [ReactiveFormsModule, DatePipe],
  templateUrl: "./appointment-panel.html",
  styleUrl: "./appointment-panel.scss",
})
export class AppointmentPanel implements OnInit {
  private auth = inject(Auth);
  private api = inject(Appointments);
  private fb = inject(FormBuilder);

  customers = input.required<CustomerDto[]>();

  appointments = signal<AppointmentDto[]>([]);
  loading = signal(true);
  saving = signal(false);
  error = signal<string | null>(null);
  formError = signal<string | null>(null);

  customerNames = computed(
    () => new Map(this.customers().map((c) => [c.id, c.name])),
  );

  sorted = computed(() =>
    [...this.appointments()].sort((a, b) => a.startAt.localeCompare(b.startAt)),
  );

  form = this.fb.group({
    title: ["", [Validators.required, Validators.maxLength(120)]],
    customerId: [null as number | null, [Validators.required]],
    startAt: ["", [Validators.required]],
    endAt: ["", [Validators.required]],
    notes: [""],
  });

  ngOnInit(): void {
    // En el servidor (SSR) no hay usuario: la carga real ocurre en el navegador.
    if (!this.auth.currentUser()) return;

    this.api.getAll().subscribe({
      next: (list) => {
        this.appointments.set(list);
        this.loading.set(false);
      },
      error: () => {
        this.error.set("Não foi possível carregar os agendamentos.");
        this.loading.set(false);
      },
    });
  }

  add(): void {
    const user = this.auth.currentUser();

    if (this.form.invalid || !user) {
      this.form.markAllAsTouched();
      return;
    }

    const v = this.form.getRawValue();

    if (new Date(v.endAt!) <= new Date(v.startAt!)) {
      this.formError.set("O fim deve ser depois do início.");
      return;
    }

    this.saving.set(true);
    this.formError.set(null);

    this.api
      .create({
        title: (v.title ?? "").trim(),
        customerId: v.customerId!,
        companyId: user.companyId,
        startAt: v.startAt!,
        endAt: v.endAt!,
        notes: v.notes?.trim() || null,
      })
      .subscribe({
        next: (created) => {
          this.appointments.update((list) => [...list, created]);
          this.form.reset({
            title: "",
            customerId: null,
            startAt: "",
            endAt: "",
            notes: "",
          });
          this.saving.set(false);
        },
        error: (err) => {
          this.formError.set(
            typeof err.error === "string"
              ? err.error
              : "Não foi possível criar o agendamento.",
          );
          this.saving.set(false);
        },
      });
  }
}
