export interface AppointmentDto {
  id: number;
  title: string;
  startAt: string;
  endAt: string;
  notes: string | null;
  isCancelled: boolean;
  companyId: number;
  customerId: number;
}

export interface CreateAppointmentDto {
  title: string;
  startAt: string;
  endAt: string;
  notes: string | null;
  companyId: number;
  customerId: number;
}
