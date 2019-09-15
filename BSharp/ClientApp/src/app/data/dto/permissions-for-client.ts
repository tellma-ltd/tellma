export class PermissionsForClient {
    [viewId: string]: {
      [action: string]: boolean;
    };
  }
