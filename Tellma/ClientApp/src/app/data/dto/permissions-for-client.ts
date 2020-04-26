export interface PermissionsForClient {
    [view: string]: {
      [action: string]: boolean;
    };
  }
