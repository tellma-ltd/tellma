import { Component, Input, OnInit } from '@angular/core';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { addToWorkspace } from '~/app/data/util';
import { tap } from 'rxjs/operators';
import { WorkspaceService } from '~/app/data/workspace.service';
import { ApiService } from '~/app/data/api.service';
import { TranslateService } from '@ngx-translate/core';
import { ActivatedRoute, ParamMap } from '@angular/router';
import { DocumentForSave, Document, serialNumber as formatSerialNumber } from '~/app/data/entities/document';
import { DocumentDefinitionForClient } from '~/app/data/dto/definitions-for-client';
import { LineForSave } from '~/app/data/entities/line';
import { EntryForSave, Entry } from '~/app/data/entities/entry';

@Component({
  selector: 'b-documents-details',
  templateUrl: './documents-details.component.html',
  styles: []
})
export class DocumentsDetailsComponent extends DetailsBaseComponent implements OnInit {

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

  public expand = `OperatingSegment,Lines/Entries/Account/Currency,Signatures/Agent,Signatures/Role,Signatures/CreatedBy`;

  constructor(
    private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService,
    private route: ActivatedRoute) {
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

  get viewId(): string {
    return `documents/${this.definitionId}`;
  }

  // UI Binding

  private get definition(): DocumentDefinitionForClient {
    return !!this.definitionId ? this.workspace.current.definitions.Documents[this.definitionId] : null;
  }

  public get found(): boolean {
    return !!this.definition;
  }

  create = () => {
    const result = new DocumentForSave();
    result.Memo = this.initialText;
    result.DocumentDate = new Date().toISOString().split('T')[0];
    result.MemoIsCommon = true;

    result.Lines = [];

    // const defs = this.definition;
    // TODO: Set defaults

    return result;
  }

  public get ws() {
    return this.workspace.current;
  }

  public get masterCrumb(): string {
    return this.ws.getMultilingualValueImmediate(this.definition, 'TitlePlural');
  }

  public formatSerial(serial: number) {
    if (!serial) {
      return `(${this.translate.instant('New')})`;
    }
    const def = this.definition;
    return formatSerialNumber(serial, def.Prefix, 4);
  }

  public get OperatingSegment_isVisible(): boolean {
    // return !!this.definition.OperatingSegmentVisibility;
    return true;
  }

  public get OperatingSegment_isRequired(): boolean {
    // return this.definition.OperatingSegmentVisibility === 'Required';
    return false;
  }

  public get OperatingSegment_label(): string {
    // return !!this.definition.OperatingSegmentLabel ?
    //   this.ws.getMultilingualValueImmediate(this.definition, 'OperatingSegmentLabel') :
    //   this.translate.instant('OperatingSegment');

    return this.translate.instant('OperatingSegment');
  }

  // TODO
  isInactive(_: DocumentForSave) {
    return null;
  }

  showLineErrors(_: Document) {
    return false;
  }

  onNewLine(item: LineForSave) {
    item.Entries = [new EntryForSave()];
    item.DefinitionId = 'ManualLine';
    return item;
  }

  getDebit(entry: Entry): number {
    return this.isDebit(entry) ? entry.Value : null;
  }

  setDebit(entry: Entry, v: number): void {
    entry.Value = v;
    entry.Direction = 1;
  }

  getCredit(entry: Entry): number {
    return this.isCredit(entry) ? entry.Value : null;
  }

  setCredit(entry: Entry, v: number): void {
    entry.Value = v;
    entry.Direction = -1;
  }

  isCredit(entry: Entry) {
    return entry.Direction === -1;
  }

  isDebit(entry: Entry) {
    return entry.Direction === 1;
  }

  public columnPaths(model: DocumentForSave): string[] {
    return model.MemoIsCommon ?
      ['AccountId', 'Debit', 'Credit', 'Dynamic'] :
      ['Memo', 'AccountId', 'Debit', 'Credit', 'Dynamic'];
  }

  public showMonetaryValue(entry: Entry): boolean {
    const account = this.ws.get('Account', entry.AccountId);
    return !!account && !!account.CurrencyId && account.CurrencyId !== this.ws.settings.FunctionalCurrencyId;
  }

  public get functionalPostfix(): string {
    return ' (' + this.ws.getMultilingualValueImmediate(this.ws.settings, 'FunctionalCurrencyName') + ')';
  }
}
