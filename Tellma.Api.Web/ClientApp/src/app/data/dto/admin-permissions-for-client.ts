import { PermissionsForClientViews, UserPermission } from './permissions-for-client';

export interface AdminPermissionsForClient {
  Permissions: UserPermission[];
  Views?: PermissionsForClientViews; // Added on the client side
}
