export interface AdminPermissionsForClient {
    [view: string]: {
      [action: string]: boolean;
    };
  }
