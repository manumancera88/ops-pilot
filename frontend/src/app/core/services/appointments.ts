import { Injectable, inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import {
  AppointmentDto,
  CreateAppointmentDto,
} from "./models/appointment.models";

const API_URL = "http://localhost:5265/api";

@Injectable({ providedIn: "root" })
export class Appointments {
  private http = inject(HttpClient);

  getAll(): Observable<AppointmentDto[]> {
    return this.http.get<AppointmentDto[]>(`${API_URL}/Appointments`);
  }

  create(dto: CreateAppointmentDto): Observable<AppointmentDto> {
    return this.http.post<AppointmentDto>(`${API_URL}/Appointments`, dto);
  }
}
