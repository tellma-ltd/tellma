// tslint:disable:variable-name
export interface UserSettingsForClient {
  UserId: number;
  ImageId: string;
  Name: string;
  Name2: string;
  Name3: string;
  PreferredLanguage: string;
  CustomSettings: { [key: string]: string };
}
