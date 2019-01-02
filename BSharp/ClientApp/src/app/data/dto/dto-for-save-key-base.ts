export abstract class DtoForSaveKeyBase {
  Id: string | number = null;
  EntityState: 'Inserted' | 'Updated' | 'Deleted' = 'Inserted';
}
