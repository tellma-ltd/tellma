import { Component, OnInit, Input } from '@angular/core';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { ApiService } from '~/app/data/api.service';
import { WorkspaceService } from '~/app/data/workspace.service';
import { Router, ActivatedRoute, ParamMap } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { DocumentDefinitionForClient } from '~/app/data/dto/definitions-for-client';

@Component({
  selector: 't-documents-master',
  templateUrl: './documents-master.component.html',
  styles: []
})
export class DocumentsMasterComponent extends MasterBaseComponent implements OnInit {

  private documentsApi = this.api.documentsApi('', this.notifyDestruct$); // for intellisense
  private _definitionId: string;

  @Input()
  public set definitionId(t: string) {
    if (this._definitionId !== t) {
      this.documentsApi = this.api.documentsApi(t, this.notifyDestruct$);
      this._definitionId = t;
    }
  }

  public get definitionId(): string {
    return this._definitionId;
  }

  public expand = '';

  constructor(
    private workspace: WorkspaceService, private api: ApiService, private router: Router,
    private route: ActivatedRoute, private translate: TranslateService) {
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
    return `documents/${this.definitionId}`;
  }

  public get c() {
    return this.ws.Document;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  public get definition(): DocumentDefinitionForClient {
    return !!this.definitionId ? this.ws.definitions.Documents[this.definitionId] : null;
  }

  public get found(): boolean {
    return !this.definitionId || !!this.definition;
  }

  public get masterCrumb(): string {
    return !!this.definition ?
      this.ws.getMultilingualValueImmediate(this.definition, 'TitlePlural') :
      this.translate.instant('Documents');
  }

  public get summary(): string {
    return !!this.definition ?
      this.ws.getMultilingualValueImmediate(this.definition, 'TitleSingular') :
      this.translate.instant('Document');
  }
}
