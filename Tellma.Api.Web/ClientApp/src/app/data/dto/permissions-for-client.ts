export interface PermissionsForClient {
  Permissions: UserPermission[];
  Views?: PermissionsForClientViews; // Added on the client side
  ReportIds: number[];
  DashboardIds: number[];
  TemplateIds: number[];
}

export interface UserPermission {
  View: string;
  Action: string;
  Criteria: string;
}

export interface PermissionsForClientViews {
  [view: string]: {
    [action: string]: boolean;
  };
}
