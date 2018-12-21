export abstract class DtoForSaveKeyBase {
  Id: string | number;
  EntityState: 'Inserting' | 'Updating' | 'Deleting';
}
