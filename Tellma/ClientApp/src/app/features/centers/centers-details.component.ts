import { Component } from '@angular/core';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { TranslateService } from '@ngx-translate/core';
import { ChoicePropDescriptor, getChoices } from '~/app/data/entities/base/metadata';
import {
  CenterForSave, Center,
  metadata_Center
} from '~/app/data/entities/center';
import { SelectorChoice } from '~/app/shared/selector/selector.component';

@Component({
  selector: 't-centers-details',
  templateUrl: './centers-details.component.html',
  styles: []
})
export class CentersDetailsComponent extends DetailsBaseComponent {

  private centersApi = this.api.centersApi(this.notifyDestruct$); // for intellisense

  public expand = 'Parent,Manager';

  create = () => {
    const result: CenterForSave = { };
    if (this.ws.isPrimaryLanguage) {
      result.Name = this.initialText;
    } else if (this.ws.isSecondaryLanguage) {
      result.Name2 = this.initialText;
    } else if (this.ws.isTernaryLanguage) {
      result.Name3 = this.initialText;
    }

    result.IsLeaf = true;

    return result;
  }

  constructor(
    private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();

    this.centersApi = this.api.centersApi(this.notifyDestruct$);
  }

  get centerTypeChoices(): SelectorChoice[] {

    const descriptor = metadata_Center(this.workspace, this.translate, null)
      .properties.CenterType as ChoicePropDescriptor;

    return getChoices(descriptor);
  }

  public centerTypeLookup(value: string): string {
    const descriptor = metadata_Center(this.workspace, this.translate, null)
      .properties.CenterType as ChoicePropDescriptor;

    return descriptor.format(value);
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  public onActivate = (model: Center): void => {
    if (!!model && !!model.Id) {
      this.centersApi.activate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onDeactivate = (model: Center): void => {
    if (!!model && !!model.Id) {
      this.centersApi.deactivate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public showActivate = (model: Center) => !!model && !model.IsActive;
  public showDeactivate = (model: Center) => !!model && model.IsActive;

  public canActivateDeactivateItem = (model: Center) => this.ws.canDo('centers', 'IsActive', model.Id);

  public activateDeactivateTooltip = (model: Center) => this.canActivateDeactivateItem(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')
}
