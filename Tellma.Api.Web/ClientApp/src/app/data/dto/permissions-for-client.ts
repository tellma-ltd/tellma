export interface PermissionsForClient {
  Views: PermissionsForClientViews;
  ReportIds: number[];
  DashboardIds: number[];
}

export interface PermissionsForClientViews {
  [view: string]: {
    [action: string]: boolean;
  };
}
