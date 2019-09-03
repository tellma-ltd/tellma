import { Component, Input, OnInit } from '@angular/core';
import { Subject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { Agent, AgentForSave, metadata_Agent } from '~/app/data/entities/agent';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { TranslateService } from '@ngx-translate/core';
import { supportedCultures } from '~/app/data/supported-cultures';
import { ChoicePropDescriptor } from '~/app/data/entities/base/metadata';

@Component({
  selector: 'b-agents-details',
  templateUrl: './agents-details.component.html',
  styleUrls: ['./agents-details.component.scss']
})
export class AgentsDetailsComponent extends DetailsBaseComponent {

  private _languageChoices: { name: string, value: any }[];
  private _agentTypeChoices: { name: string, value: any }[];
  private notifyDestruct$ = new Subject<void>();
  private agentsApi = this.api.agentsApi(this.notifyDestruct$); // for intellisense

  public expand = 'User';

  create = () => {
    const result = new AgentForSave();
    result.Name = this.initialText;
    result.PreferredLanguage = this.ws.settings.PrimaryLanguageId;
    return result;
  }

  constructor(private workspace: WorkspaceService, private api: ApiService,
    private translate: TranslateService) {
    super();
  }

  get languageChoices(): { name: string, value: any }[] {

    if (!this._languageChoices) {
      this._languageChoices = [{ name: this.ws.settings.PrimaryLanguageName, value: this.ws.settings.PrimaryLanguageId }];
      if (!!this.ws.settings.SecondaryLanguageId) {
        this._languageChoices.push({
          name: this.ws.settings.SecondaryLanguageName,
          value: this.ws.settings.SecondaryLanguageId
        });
      }
      if (!!this.ws.settings.TernaryLanguageId) {
        this._languageChoices.push({
          name: this.ws.settings.TernaryLanguageName,
          value: this.ws.settings.TernaryLanguageId
        });
      }
    }

    return this._languageChoices;
  }

  public languageLookup(value: string) {
    return supportedCultures[value];
  }

  get agentTypeChoices(): { name: string, value: any }[] {
    if (!this._agentTypeChoices) {
      const descriptor = <ChoicePropDescriptor> metadata_Agent(this.ws, this.translate, null).properties.AgentType;
      this._agentTypeChoices = descriptor.choices.map(c => ({ name: descriptor.format(c), value: c }));
    }

    return this._agentTypeChoices;
  }

  public agentTypeLookup(value: string): string {
    if (!value) {
      return '';
    }

    const descriptor = <ChoicePropDescriptor> metadata_Agent(this.ws, this.translate, null).properties.AgentType;
    return descriptor.format(value);
  }

  public onActivate = (model: Agent): void => {
    if (!!model && !!model.Id) {
      this.agentsApi.activate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe(null, this.details.handleActionError);
    }
  }

  public onDeactivate = (model: Agent): void => {
    if (!!model && !!model.Id) {
      this.agentsApi.deactivate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe(null, this.details.handleActionError);
    }
  }

  public showActivate = (model: Agent) => !!model && !model.IsActive;
  public showDeactivate = (model: Agent) => !!model && model.IsActive;

  public canActivateDeactivateItem = (model: Agent) => this.ws.canDo('agents', 'IsActive', model.Id);

  public activateDeactivateTooltip = (model: Agent) => this.canActivateDeactivateItem(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  public get ws() {
    return this.workspace.current;
  }
}
