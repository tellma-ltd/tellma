// tslint:disable:variable-name
// tslint:disable:no-empty-interface

export interface GlobalSettingsForClient {
    EmailEnabled: boolean;
    SmsEnabled: boolean;
    PushEnabled: boolean;

    CanInviteUsers: boolean;
    TokenExpiryInDays: number;
}
