import { Component, Input, OnInit } from '@angular/core';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { Lookup, LookupForSave } from '~/app/data/entities/lookup';
import { addToWorkspace } from '~/app/data/util';
import { tap } from 'rxjs/operators';
import { WorkspaceService } from '~/app/data/workspace.service';
import { ApiService } from '~/app/data/api.service';
import { TranslateService } from '@ngx-translate/core';
import { Router, ActivatedRoute, ParamMap } from '@angular/router';

@Component({
  selector: 't-lookups-details',
  templateUrl: './lookups-details.component.html',
  styles: []
})
export class LookupsDetailsComponent extends DetailsBaseComponent implements OnInit {

  private lookupsApi = this.api.lookupsApi('', this.notifyDestruct$); // for intellisense
  private _definitionId: string;

  @Input()
  public set definitionId(t: string) {
    if (this._definitionId !== t) {
      this._definitionId = t;
      this.lookupsApi = this.api.lookupsApi(t, this.notifyDestruct$);
    }
  }

  public get definitionId(): string {
    return this._definitionId;
  }

  public expand = '';

  constructor(
    private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService,
    private router: Router, private route: ActivatedRoute) {
    super();
  }

  ngOnInit() {
    this.route.paramMap.subscribe((params: ParamMap) => {
      // This triggers changes on the screen

      if (this.isScreenMode) {

        const definitionId = params.get('definitionId');

        if (!definitionId || !this.workspace.current.definitions.Lookups[definitionId]) {
          this.router.navigate(['page-not-found'], { relativeTo: this.route.parent, replaceUrl: true });
        }

        if (this.definitionId !== definitionId) {
          this.definitionId = definitionId;
        }
      }
    });
  }
  // UI Binding

  create = () => {
    const result: LookupForSave = { };
    if (this.ws.isPrimaryLanguage) {
      result.Name = this.initialText;
    } else if (this.ws.isSecondaryLanguage) {
      result.Name2 = this.initialText;
    } else if (this.ws.isTernaryLanguage) {
      result.Name3 = this.initialText;
    }
    return result;
  }

  public get ws() {
    return this.workspace.current;
  }

  public onActivate = (model: Lookup): void => {
    if (!!model && !!model.Id) {
      this.lookupsApi.activate([model.Id], { returnEntities: true, expand: this.expand }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onDeactivate = (model: Lookup): void => {
    if (!!model && !!model.Id) {
      this.lookupsApi.deactivate([model.Id], { returnEntities: true, expand: this.expand }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public showActivate = (model: Lookup) => !!model && !model.IsActive;
  public showDeactivate = (model: Lookup) => !!model && model.IsActive;

  public canActivateDeactivateItem = (model: Lookup) => this.ws.canDo(this.definitionId, 'IsActive', model.Id);

  public activateDeactivateTooltip = (model: Lookup) => this.canActivateDeactivateItem(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  public get masterCrumb(): string {
    const definitionId = this.definitionId;
    const definition = this.workspace.current.definitions.Lookups[definitionId];
    if (!definition) {
      this.router.navigate(['page-not-found'], { relativeTo: this.route.parent, replaceUrl: true });
    }

    return this.ws.getMultilingualValueImmediate(definition, 'TitlePlural');
  }
}
