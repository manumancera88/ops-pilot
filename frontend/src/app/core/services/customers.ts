import { Injectable, inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { CustomerDto, CreateCustomerDto } from "./models/customer.models";

const API_URL = "http://localhost:5265/api";

@Injectable({ providedIn: "root" })
export class Customers {
  private http = inject(HttpClient);

  getAll(): Observable<CustomerDto[]> {
    return this.http.get<CustomerDto[]>(`${API_URL}/Customers`);
  }

  create(dto: CreateCustomerDto): Observable<CustomerDto> {
    return this.http.post<CustomerDto>(`${API_URL}/Customers`, dto);
  }
}
