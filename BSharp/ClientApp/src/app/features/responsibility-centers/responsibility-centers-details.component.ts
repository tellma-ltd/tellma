import { Component } from '@angular/core';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { TranslateService } from '@ngx-translate/core';
import { ChoicePropDescriptor, getChoices } from '~/app/data/entities/base/metadata';
import { ActivatedRoute } from '@angular/router';
import {
  ResponsibilityCenterForSave, ResponsibilityCenter,
  metadata_ResponsibilityCenter
} from '~/app/data/entities/responsibility-center';
import { SelectorChoice } from '~/app/shared/selector/selector.component';

@Component({
  selector: 'b-responsibility-centers-details',
  templateUrl: './responsibility-centers-details.component.html',
  styles: []
})
export class ResponsibilityCentersDetailsComponent extends DetailsBaseComponent {

  private responsibilityCenterApi = this.api.responsibilityCenterApi(this.notifyDestruct$); // for intellisense

  public expand = 'Parent,Manager';

  create = () => {
    const result = new ResponsibilityCenterForSave();
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

    this.responsibilityCenterApi = this.api.responsibilityCenterApi(this.notifyDestruct$);
  }

  get responsibilityTypeChoices(): SelectorChoice[] {

    const descriptor = metadata_ResponsibilityCenter(this.ws, this.translate, null)
      .properties.ResponsibilityType as ChoicePropDescriptor;

    return getChoices(descriptor);
  }

  public responsibilityTypeLookup(value: string): string {
    const descriptor = metadata_ResponsibilityCenter(this.ws, this.translate, null)
      .properties.ResponsibilityType as ChoicePropDescriptor;

    return descriptor.format(value);
  }

  public get ws() {
    return this.workspace.current;
  }

  public onActivate = (model: ResponsibilityCenter): void => {
    if (!!model && !!model.Id) {
      this.responsibilityCenterApi.activate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onDeactivate = (model: ResponsibilityCenter): void => {
    if (!!model && !!model.Id) {
      this.responsibilityCenterApi.deactivate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public showActivate = (model: ResponsibilityCenter) => !!model && !model.IsActive;
  public showDeactivate = (model: ResponsibilityCenter) => !!model && model.IsActive;

  public canActivateDeactivateItem = (model: ResponsibilityCenter) => this.ws.canDo('responsibility-centers', 'IsActive', model.Id);

  public activateDeactivateTooltip = (model: ResponsibilityCenter) => this.canActivateDeactivateItem(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')
}
