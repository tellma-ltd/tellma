import { Component } from '@angular/core';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { ApiService } from '~/app/data/api.service';
import { WorkspaceService } from '~/app/data/workspace.service';
import { tap } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { addToWorkspace } from '~/app/data/util';

@Component({
  selector: 'b-product-categories-master',
  templateUrl: './product-categories-master.component.html'
})
export class ProductCategoriesMasterComponent extends MasterBaseComponent {

  private productCategoriesApi = this.api.productCategoriesApi(this.notifyDestruct$); // for intellisense

  public expand = '';

  constructor(private workspace: WorkspaceService, private api: ApiService) {
    super();
    this.productCategoriesApi = this.api.productCategoriesApi(this.notifyDestruct$);
  }

  public get c() {
    return this.workspace.current.ProductCategory;
  }

  public get ws() {
    return this.workspace.current;
  }
  public onActivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.productCategoriesApi.activate(ids, { returnEntities: true, expand: this.expand }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public onDeactivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.productCategoriesApi.deactivate(ids, { returnEntities: true, expand: this.expand }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }
}
