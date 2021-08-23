// tslint:disable:variable-name
export interface AdminUserSettingsForClient {
  UserId: number;
  Name: string;
  CustomSettings: { [key: string]: string };
}
