export interface GetArguments {
  top?: number;
  skip?: number;
  orderby?: string;
  desc?: boolean;
  search?: string;
  filter?: string;
  expand?: string;
  select?: string;
  selectTemplate?: string;
  countEntities?: boolean;
  unobtrusive?: boolean;
}
