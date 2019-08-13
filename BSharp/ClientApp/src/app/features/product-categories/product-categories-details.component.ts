import { Component, OnInit } from '@angular/core';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { Subject } from 'rxjs';
import { ProductCategoryForSave, ProductCategory } from '~/app/data/dto/product-category';
import { ApiService } from '~/app/data/api.service';
import { WorkspaceService } from '~/app/data/workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { tap } from 'rxjs/operators';
import { addToWorkspace } from '~/app/data/util';

@Component({
  selector: 'b-product-categories-details',
  templateUrl: './product-categories-details.component.html',
  styleUrls: ['./product-categories-details.component.scss']
})
export class ProductCategoriesDetailsComponent extends DetailsBaseComponent {

  private notifyDestruct$ = new Subject<void>();
  private productCategoriesApi = this.api.productCategoriesApi(this.notifyDestruct$); // for intellisense

  public expand = 'Parent';

  create = () => {
    const result = new ProductCategoryForSave();
    return result;
  }

  constructor(private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();

    this.productCategoriesApi = this.api.productCategoriesApi(this.notifyDestruct$);
  }

  public get ws() {
    return this.workspace.current;
  }

  public onActivate = (model: ProductCategory): void => {
    if (!!model && !!model.Id) {
      this.productCategoriesApi.activate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe(null, this.details.handleActionError);
    }
  }

  public onDeactivate = (model: ProductCategory): void => {
    if (!!model && !!model.Id) {
      this.productCategoriesApi.deactivate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe(null, this.details.handleActionError);
    }
  }

  public showActivate = (model: ProductCategory) => !!model && !model.IsActive;
  public showDeactivate = (model: ProductCategory) => !!model && model.IsActive;

  public canActivateDeactivateItem = (model: ProductCategory) => this.ws.canDo('product-categories', 'IsActive', model.Id);

  public activateDeactivateTooltip = (model: ProductCategory) => this.canActivateDeactivateItem(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')
}
