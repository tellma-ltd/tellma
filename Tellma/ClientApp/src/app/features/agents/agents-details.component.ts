import { Component, Input, OnInit } from '@angular/core';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { Agent, AgentForSave } from '~/app/data/entities/agent';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { TranslateService } from '@ngx-translate/core';
import { ActivatedRoute, ParamMap } from '@angular/router';
import { AgentDefinitionForClient } from '~/app/data/dto/definitions-for-client';

@Component({
  selector: 't-agents-details',
  templateUrl: './agents-details.component.html'
})
export class AgentsDetailsComponent extends DetailsBaseComponent implements OnInit {

  private agentsApi = this.api.agentsApi('', this.notifyDestruct$); // for intellisense
  private _definitionId: string;

  @Input()
  public set definitionId(t: string) {
    if (this._definitionId !== t) {
      this.agentsApi = this.api.agentsApi(t, this.notifyDestruct$);
      this._definitionId = t;
    }
  }

  public get definitionId(): string {
    return this._definitionId;
  }

  public expand = 'User';

  create = () => {
    const result: AgentForSave = {};
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
    private translate: TranslateService, private route: ActivatedRoute) {
    super();
  }

  ngOnInit() {
    this.route.paramMap.subscribe((params: ParamMap) => {
      // This triggers changes on the screen

      if (this.isScreenMode) {

        const definitionId = params.get('definitionId');

        if (this.definitionId !== definitionId) {
          this.definitionId = definitionId;
        }
      }
    });
  }

  get view(): string {
    return `agents/${this.definitionId}`;
  }

  private get definition(): AgentDefinitionForClient {
    return !!this.definitionId ? this.ws.definitions.Agents[this.definitionId] : null;
  }

  // UI Bindings

  public get found(): boolean {
    return !!this.definition;
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

  public canActivateDeactivateItem = (model: Agent) => this.ws.canDo(this.view, 'IsActive', model.Id);

  public activateDeactivateTooltip = (model: Agent) => this.canActivateDeactivateItem(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  public get ws() {
    return this.workspace.currentTenant;
  }

  public get masterCrumb(): string {
    return this.ws.getMultilingualValueImmediate(this.definition, 'TitlePlural');
  }

  public get TaxIdentificationNumber_isVisible(): boolean {
    return !!this.definition.TaxIdentificationNumberVisibility;
  }

  public get TaxIdentificationNumber_isRequired(): boolean {
    return this.definition.TaxIdentificationNumberVisibility === 'Required';
  }

  public get StartDate_isVisible(): boolean {
    return !!this.definition.StartDateVisibility;
  }

  public get StartDate_isRequired(): boolean {
    return this.definition.StartDateVisibility === 'Required';
  }

  public get StartDate_label(): string {
    return !!this.definition.StartDateLabel ?
      this.ws.getMultilingualValueImmediate(this.definition, 'StartDateLabel') :
      this.translate.instant('Agent_StartDate');
  }

  public get Job_isVisible(): boolean {
    return !!this.definition.JobVisibility;
  }

  public get Job_isRequired(): boolean {
    return this.definition.JobVisibility === 'Required';
  }

  public get BasicSalary_isVisible(): boolean {
    return !!this.definition.BasicSalaryVisibility;
  }

  public get BasicSalary_isRequired(): boolean {
    return this.definition.BasicSalaryVisibility === 'Required';
  }

  public get TransportationAllowance_isVisible(): boolean {
    return !!this.definition.TransportationAllowanceVisibility;
  }

  public get TransportationAllowance_isRequired(): boolean {
    return this.definition.TransportationAllowanceVisibility === 'Required';
  }

  public get OvertimeRate_isVisible(): boolean {
    return !!this.definition.OvertimeRateVisibility;
  }

  public get OvertimeRate_isRequired(): boolean {
    return this.definition.OvertimeRateVisibility === 'Required';
  }

  public get BankAccountNumber_isVisible(): boolean {
    return !!this.definition.BankAccountNumberVisibility;
  }

  public get BankAccountNumber_isRequired(): boolean {
    return this.definition.BankAccountNumberVisibility === 'Required';
  }
}
