import { Component, Input, OnInit } from '@angular/core';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { Agent, AgentForSave, metadata_Agent } from '~/app/data/entities/agent';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { TranslateService } from '@ngx-translate/core';
import { ChoicePropDescriptor } from '~/app/data/entities/base/metadata';
import { Router, ActivatedRoute, Params, ParamMap } from '@angular/router';
import { SelectorChoice } from '~/app/shared/selector/selector.component';
import { AgentDefinitionForClient } from '~/app/data/dto/definitions-for-client';

@Component({
  selector: 'b-agents-details',
  templateUrl: './agents-details.component.html'
})
export class AgentsDetailsComponent extends DetailsBaseComponent implements OnInit {

  private _agentTypeChoices: SelectorChoice[];
  private agentsApi = this.api.agentsApi('', this.notifyDestruct$); // for intellisense
  private _definitionId: string;


  @Input()
  public set definitionId(t: string) {
    if (this._definitionId !== t) {
      this._definitionId = t;
      this.agentsApi = this.api.agentsApi(t, this.notifyDestruct$);
    }
  }

  public get definitionId(): string {
    return this._definitionId;
  }

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
    result.IsRelated = false;

// TODO Set defaults from definition

    return result;
  }

  constructor(
    private workspace: WorkspaceService, private api: ApiService,
    private translate: TranslateService, private router: Router, private route: ActivatedRoute) {
    super();
  }

  ngOnInit() {
    this.route.paramMap.subscribe((params: ParamMap) => {
      // This triggers changes on the screen

      if (this.isScreenMode) {

        const definitionId = params.get('definitionId');

        if (!definitionId || !this.workspace.current.definitions.Agents[definitionId]) {
          this.router.navigate(['page-not-found'], { relativeTo: this.route.parent, replaceUrl: true });
        }

        if (this.definitionId !== definitionId) {
          this.definitionId = definitionId;
        }
      }
    });
  }

  get viewId(): string {
    return `agents/${this.definitionId}`;
  }

  public get d(): AgentDefinitionForClient {
    return this.ws.definitions.Agents[this.definitionId];
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

  public canActivateDeactivateItem = (model: Agent) => this.ws.canDo(this.viewId, 'IsActive', model.Id);

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
        agent_id: model.Id
      };

      this.router.navigate(['../../users/new', params], { relativeTo: this.route });
    }
  }

  public get masterCrumb(): string {
    const definitionId = this.definitionId;
    const definition = this.workspace.current.definitions.Agents[definitionId];
    if (!definition) {
      this.router.navigate(['page-not-found'], { relativeTo: this.route.parent, replaceUrl: true });
    }

    return this.ws.getMultilingualValueImmediate(definition, 'TitlePlural');
  }
}
