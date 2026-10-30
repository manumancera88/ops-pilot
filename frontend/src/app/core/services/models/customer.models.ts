export interface CustomerDto {
  id: number;
  name: string;
  email: string | null;
  phone: string | null;
  isActive: boolean;
  companyId: number;
}

export interface CreateCustomerDto {
  name: string;
  email: string | null;
  phone: string | null;
}
