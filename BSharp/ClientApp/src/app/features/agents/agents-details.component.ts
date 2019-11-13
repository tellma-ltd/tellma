import { Component } from '@angular/core';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { Agent, AgentForSave, metadata_Agent } from '~/app/data/entities/agent';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { TranslateService } from '@ngx-translate/core';
import { supportedCultures } from '~/app/data/supported-cultures';
import { ChoicePropDescriptor } from '~/app/data/entities/base/metadata';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { SelectorChoice } from '~/app/shared/selector/selector.component';

@Component({
  selector: 'b-agents-details',
  templateUrl: './agents-details.component.html'
})
export class AgentsDetailsComponent extends DetailsBaseComponent {

  private _languageChoices: SelectorChoice[];
  private _agentTypeChoices: SelectorChoice[];
  private agentsApi = this.api.agentsApi(this.notifyDestruct$); // for intellisense

  public expand = 'User';

  create = () => {
    const result = new AgentForSave();
    if (this.ws.isPrimaryLanguage) {
      result.Name = this.initialText;
    } else if (this.ws.isSecondaryLanguage) {
      result.Name2 = this.initialText;
    } else if (this.ws.isTernaryLanguage) {
      result.Name3 = this.initialText;
    }
    result.AgentType = 'Individual';
    result.IsRelated = false;
    result.PreferredLanguage = this.ws.settings.PrimaryLanguageId;
    return result;
  }

  constructor(private workspace: WorkspaceService, private api: ApiService,
              private translate: TranslateService, private router: Router, private route: ActivatedRoute) {
    super();
  }

  get languageChoices(): SelectorChoice[] {

    if (!this._languageChoices) {
      this._languageChoices = [{ name: () => this.ws.settings.PrimaryLanguageName, value: this.ws.settings.PrimaryLanguageId }];
      if (!!this.ws.settings.SecondaryLanguageId) {
        this._languageChoices.push({
          name: () => this.ws.settings.SecondaryLanguageName,
          value: this.ws.settings.SecondaryLanguageId
        });
      }
      if (!!this.ws.settings.TernaryLanguageId) {
        this._languageChoices.push({
          name: () => this.ws.settings.TernaryLanguageName,
          value: this.ws.settings.TernaryLanguageId
        });
      }
    }

    return this._languageChoices;
  }

  public languageLookup(value: string) {
    return supportedCultures[value];
  }

  get agentTypeChoices(): SelectorChoice[] {
    if (!this._agentTypeChoices) {
      const descriptor = metadata_Agent(this.ws, this.translate, null).properties.AgentType as ChoicePropDescriptor;
      this._agentTypeChoices = descriptor.choices.map(c => ({ name: () => descriptor.format(c), value: c }));
    }

    return this._agentTypeChoices;
  }

  public agentTypeLookup(value: string): string {
    if (!value) {
      return '';
    }

    const descriptor = metadata_Agent(this.ws, this.translate, null).properties.AgentType as ChoicePropDescriptor;
    return descriptor.format(value);
  }

  public onActivate = (model: Agent): void => {
    if (!!model && !!model.Id) {
      this.agentsApi.activate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onDeactivate = (model: Agent): void => {
    if (!!model && !!model.Id) {
      this.agentsApi.deactivate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
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

  public showUser(model: Agent): boolean {
    return !!model && !!this.workspace.current.get('User', model.Id);
  }

  public createUser(model: Agent): void {
    if (!!model && !!model.Id) {
      const params: Params = {
        agent_id : model.Id
      };

      this.router.navigate(['../../users/new', params], { relativeTo : this.route });
    }
  }
}
