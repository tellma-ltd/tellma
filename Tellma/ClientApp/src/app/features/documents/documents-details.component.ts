import { Component, Input, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { WorkspaceService, TenantWorkspace } from '~/app/data/workspace.service';
import { ApiService } from '~/app/data/api.service';
import { TranslateService } from '@ngx-translate/core';
import { ActivatedRoute, ParamMap } from '@angular/router';
import { DocumentForSave, Document, serialNumber } from '~/app/data/entities/document';
import { DocumentDefinitionForClient, ResourceDefinitionForClient } from '~/app/data/dto/definitions-for-client';
import { LineForSave } from '~/app/data/entities/line';
import { Entry } from '~/app/data/entities/entry';
import { DocumentAssignment } from '~/app/data/entities/document-assignment';
import { addToWorkspace, getDataURL, downloadBlob, fileSizeDisplay, mergeEntitiesInWorkspace } from '~/app/data/util';
import { tap, catchError, finalize, takeUntil } from 'rxjs/operators';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { of, throwError } from 'rxjs';
import { AccountForSave } from '~/app/data/entities/account';
import { Resource } from '~/app/data/entities/resource';
import { Currency } from '~/app/data/entities/currency';
import { metadata_Agent } from '~/app/data/entities/agent';
import { AccountType } from '~/app/data/entities/account-type';
import { Attachment } from '~/app/data/entities/attachment';
import { MeasurementUnit } from '~/app/data/entities/measurement-unit';
import { EntityWithKey } from '~/app/data/entities/base/entity-with-key';
import { RequiredSignature } from '~/app/data/entities/required-signature';

interface DocumentEventBase {
  time: string;
  userId: number;
}

interface DocumentReassignmentEvent extends DocumentEventBase {
  type: 'reassignment';
  assigneeId: number;
  comment?: string;
}

interface DocumentCreationEvent extends DocumentEventBase {
  type: 'creation';
}

type DocumentEvent = DocumentReassignmentEvent | DocumentCreationEvent;

@Component({
  selector: 't-documents-details',
  templateUrl: './documents-details.component.html',
  styles: []
})
export class DocumentsDetailsComponent extends DetailsBaseComponent implements OnInit {

  private documentsApi = this.api.documentsApi('', this.notifyDestruct$); // for intellisense
  private _definitionId: string;
  private _currentDoc: Document;
  private _sortedHistory: { date: string, events: DocumentEvent[] }[] = [];
  private _maxAttachmentSize = 20 * 1024 * 1024;
  private _pristineDocJson: string;

  // Required signature stuff
  private _requiredSignaturesDetailed: RequiredSignature[];
  private _requiredSignaturesLineIds: number[];
  private _requiredSignaturesSummary: RequiredSignature[];
  private _requiredSignaturesLineIdsHash: HashTable;
  private _requiredSignatureProps = [
    'ToState', 'RuleType', 'RoleId', 'SignedById', 'SignedAt',
    'OnBehalfOfUserId', 'CanSign', 'ProxyRoleId', 'CanSignOnBehalf'];

  private _requiredSignaturesForLineDefModel: Document;
  private _requiredSignaturesForLineDefLineDef: string;
  private _requiredSignaturesForLineDefLineIds: number[];


  // These are bound from UI
  public assigneeId: number;
  public comment: string;
  public picSize = 36;


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

  @ViewChild('confirmModal', { static: true })
  confirmModal: TemplateRef<any>;

  public confirmationMessage: string;

  public expand = 'CreatedBy,ModifiedBy,Assignee,' +
    // Entry Account
    ['Currency', /* 'Resource/Currency', */ 'Resource/Units', 'Agent',
      'EntryType', 'AccountType', 'ResponsibilityCenter'] // , 'Resource/ResourceClassification', 'ResourceClassification']
      .map(prop => `Lines/Entries/Account/${prop}`).join(',') + ',' +

    // Entry
    ['Currency', 'Resource/Currency', 'Resource/Units', 'Agent',
      'EntryType', 'NotedAgent', 'ResponsibilityCenter', 'Unit'] // , 'Resource/ResourceClassification']
      .map(prop => `Lines/Entries/${prop}`).join(',') + ',' +

    // // Signatures
    // ['OnBehalfOfUser', 'Role', 'CreatedBy']
    //   .map(prop => `Signatures/${prop}`).join(',') + ',' +

    // Attachments
    ['CreatedBy', 'ModifiedBy']
      .map(prop => `Attachments/${prop}`).join(',') + ',' +

    // Assignment history
    ['Assignee', 'CreatedBy']
      .map(prop => `AssignmentsHistory/${prop}`).join(',');

  constructor(
    private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService,
    private route: ActivatedRoute, private modalService: NgbModal) {
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

  // UI Binding

  private get definition(): DocumentDefinitionForClient {
    return !!this.definitionId ? this.ws.definitions.Documents[this.definitionId] : null;
  }

  public get found(): boolean {
    return !!this.definition;
  }

  create = () => {
    const result: DocumentForSave = {
      Memo: this.initialText,
      DocumentDate: new Date().toISOString().split('T')[0],
      MemoIsCommon: true,
      Lines: [],
      Attachments: []
    };

    // const defs = this.definition;
    // TODO: Set defaults

    return result;
  }

  clone: (item: Document) => Document = (item: Document) => {
    if (!!item) {
      const clone = JSON.parse(JSON.stringify(item)) as Document;
      clone.Id = null;

      if (!!clone.Lines) {
        clone.Lines.forEach(line => {
          line.Id = null;
          if (!!line.Entries) {
            line.Entries.forEach(entry => {
              entry.Id = null;
            });
          }
        });
      }

      clone.Attachments = [];
      clone.AssignmentsHistory = [];

      delete clone.AssigneeId;
      delete clone.CreatedById;
      delete clone.ModifiedById;
      delete clone.SerialNumber;
      delete clone.State;

      return clone;
    } else {
      // programmer mistake
      console.error('Cloning a non existing item');
      return null;
    }
  }

  public get ws(): TenantWorkspace {
    return this.workspace.currentTenant;
  }

  public get masterCrumb(): string {
    return this.ws.getMultilingualValueImmediate(this.definition, 'TitlePlural');
  }

  public formatSerial(serial: number) {
    if (!serial) {
      return `(${this.translate.instant('New')})`;
    }
    const def = this.definition;
    return serialNumber(serial, def.Prefix, 4);
  }

  // TODO
  isInactive(_: DocumentForSave) {
    return null;
  }

  showLineErrors(_: Document) {
    return false;
  }

  onNewLine(item: LineForSave) {
    item.DefinitionId = 'ManualLine';
    item.Entries = [{}];
    item.Entries[0].Direction = 1;
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

  public get functional_decimals(): number {
    return this.ws.settings.FunctionalCurrencyDecimals;
  }

  public get functional_format(): string {
    const decimals = this.functional_decimals;
    return `1.${decimals}-${decimals}`;
  }

  public columnPaths(model: DocumentForSave): string[] {
    const paths = ['AccountId', 'Debit', 'Credit'];

    if (this.ws.settings.IsMultiResponsibilityCenter) {
      paths.splice(1, 0, 'ResponsibilityCenter');
    }

    if (!model.MemoIsCommon) {
      paths.unshift('Memo');
    }

    return paths;
  }

  public get functionalPostfix(): string {
    return ' (' + this.ws.getMultilingualValueImmediate(this.ws.settings, 'FunctionalCurrencyName') + ')';
  }

  public get flip() {
    // this is to flip the UI icons in RTL
    return this.workspace.ws.isRtl ? 'horizontal' : null;
  }

  public sortChronologically(model: Document): { date: string, events: DocumentEvent[] }[] {
    if (!model) {
      return null;
    }

    if (model !== this._currentDoc) {
      this._currentDoc = model;
      const history = model.AssignmentsHistory || [];

      const filteredHistory: DocumentAssignment[] = history; // .filter(e => e.CreatedById !== e.AssigneeId);

      const mappedHistory: DocumentEvent[] = filteredHistory.map(e =>
        ({
          type: 'reassignment',
          time: e.CreatedAt,
          userId: e.CreatedById,
          assigneeId: e.AssigneeId,
          comment: e.Comment,
        }));

      if (!!model.CreatedById) {
        mappedHistory.push({ type: 'creation', userId: model.CreatedById, time: model.CreatedAt });
      }

      const sortedHistory: DocumentEvent[] = mappedHistory.sort((a, b) => {
        return a.time < b.time ? 1 :
          a.time > b.time ? -1 : 0;
      });

      const result: { [date: string]: DocumentEvent[] } = {};
      for (const entry of sortedHistory) {
        const date = entry.time.split('T')[0];
        if (!result[date]) {
          result[date] = [];
        }

        result[date].push(entry);
      }

      this._sortedHistory = Object.keys(result).map(date => ({ date, events: result[date] }));
    }

    return this._sortedHistory;
  }

  public reassignment(event: DocumentEvent): DocumentReassignmentEvent {
    return event as DocumentReassignmentEvent;
  }

  public showAssignDocument(doc: Document) {
    // return true;
    return !!doc && !!doc.AssigneeId; // === this.ws.userSettings.UserId;
  }

  public onAssign(doc: Document): void {
    if (!!this.assigneeId) {
      this.documentsApi.assign([doc.Id], {
        returnEntities: true,
        expand: this.expand,
        assigneeId: this.assigneeId,
        comment: this.comment
      }, { includeRequiredSignatures: true }).pipe(
        tap(res => {
          addToWorkspace(res, this.workspace);
          this.details.state.extras = res.Extras;
          this.handleFreshExtras(res.Extras);
        })
      ).subscribe({ error: this.details.handleActionError });

    }
  }

  public canAssign(_: Document, isEdit: boolean) {
    return !!this.assigneeId && !isEdit;
  }

  // Used by signatures and assignments
  public get marginLeft(): number {
    return this.workspace.ws.isRtl ? 0 : 36;
  }

  public get marginRight(): number {
    return this.workspace.ws.isRtl ? 36 : 0;
  }

  public account(entry: Entry): AccountForSave {
    return this.ws.get('Account', entry.AccountId) as AccountForSave;
  }

  public resource(entry: Entry): Resource {
    const account = this.account(entry);
    const accountResourceId = !!account ? account.ResourceId : null;
    const resourceId = accountResourceId || entry.ResourceId;
    return this.ws.get('Resource', resourceId) as Resource;
  }

  private resourceDefinition(entry: Entry): ResourceDefinitionForClient {
    const resource = this.resource(entry);
    const defId = !!resource ? resource.DefinitionId : null;
    const resourceDefinition = !!defId ? this.ws.definitions.Resources[defId] : null;
    return resourceDefinition;
  }

  // AgentId

  public showAgent(entry: Entry): boolean {
    const account = this.account(entry);
    return !!account && !!account.AgentDefinitionId;
  }

  public readonlyAgent(entry: Entry): boolean {
    const account = this.account(entry);
    return !!account && !!account.AgentId;
  }

  public readonlyValueAgentId(entry: Entry): number {
    const account = this.account(entry);
    return !!account ? account.AgentId : null;
  }

  public labelAgent(entry: Entry): string {
    const account = this.account(entry);
    const agentDefinitionId = !!account ? account.AgentDefinitionId : null;

    return metadata_Agent(this.workspace, this.translate, agentDefinitionId).titleSingular();
  }

  // ResourceId

  public showResource(entry: Entry): boolean {
    const account = this.account(entry);
    return !!account && account.HasResource;
  }

  public readonlyResource(entry: Entry): boolean {
    const account = this.account(entry);
    return !!account && !!account.ResourceId;
  }

  public readonlyValueResourceId(entry: Entry): number {
    const account = this.account(entry);
    return !!account ? account.ResourceId : null;
  }

  public filterResource(entry: Entry): string {
    const account = this.account(entry);
    const accountType = this.ws.get('AccountType', account.AccountTypeId) as AccountType;

    if (!!accountType.IsResourceClassification) {
      return `AccountType/Node descof ${account.AccountTypeId}`;
    } else {
      return null;
    }
  }

  public labelResource(_: Entry): string {
    return this.translate.instant('Resource');
  }

  // Quantity + Unit

  public showQuantityAndUnit(entry: Entry): boolean {
    const account = this.account(entry);
    const resource = this.resource(entry);
    return !!account && !!resource && !!resource.Units && resource.Units.length > 0;
  }

  public readonlyUnit(entry: Entry): boolean {
    const resource = this.resource(entry);
    return !!resource && !!resource.Units && resource.Units.length === 1;
  }

  public readonlyValueUnitId(entry: Entry): number {
    const resource = this.resource(entry);
    return !!resource && !!resource.Units && !!resource.Units[0] ? resource.Units[0].UnitId : null;
  }

  public filterUnitId(entry: Entry): string {
    const resource = this.resource(entry);
    if (!!resource && !!resource.Units) {
      return resource.Units.map(e => `Id eq ${e.UnitId}`).join(' or ');
    }

    return null;
  }

  private unit(entry: Entry): MeasurementUnit {
    const unitId = this.readonlyUnit(entry) ? this.readonlyValueUnitId(entry) : entry.UnitId;
    return this.ws.get('MeasurementUnit', unitId) as MeasurementUnit;
  }

  // DueDate

  public showDueDate(entry: Entry): boolean {
    const resourceDefinition = this.resourceDefinition(entry);
    return !!resourceDefinition && !!resourceDefinition.DueDateVisibility;
  }

  public requireDueDate(entry: Entry): boolean {
    const resourceDefinition = this.resourceDefinition(entry);
    return !!resourceDefinition && resourceDefinition.DueDateVisibility === 'Required';
  }

  public labelDueDate(entry: Entry): string {
    const resourceDefinition = this.resourceDefinition(entry);
    return this.ws.getMultilingualValueImmediate(resourceDefinition, 'DueDateLabel');
  }

  // Currency

  private getAccountResourceCurrencyId(entry: Entry): string {
    // returns the currency Id (if any) that will eventually be copied to the Entry in the bll
    if (!entry) {
      return null;
    }

    const account = this.account(entry);
    const resource = this.ws.get('Resource', entry.ResourceId) as Resource;

    const accountCurrencyId = !!account ? account.CurrencyId : null;
    const resourceCurrencyId = !!resource ? resource.CurrencyId : null;

    return accountCurrencyId || resourceCurrencyId;
  }

  public showCurrency(entry: Entry): boolean {
    const account = this.account(entry);
    return !!account && !this.getAccountResourceCurrencyId(entry);
  }

  public readonlyValueCurrencyId(entry: Entry): string {
    // returns the currency Id if any
    if (!entry) {
      return null;
    }

    const accountResourceCurrencyId = this.getAccountResourceCurrencyId(entry);
    const entryCurrencyId = entry.CurrencyId;

    return accountResourceCurrencyId || entryCurrencyId;
  }

  public get functionalId(): string {
    return this.ws.settings.FunctionalCurrencyId;
  }

  // MonetaryValue

  public showMonetaryValue(entry: Entry): boolean {
    const account = this.account(entry);
    const currencyId = this.readonlyValueCurrencyId(entry);
    return !!account && !!currencyId && currencyId !== this.functionalId;
  }

  public MonetaryValue_decimals(entry: Entry): number {
    const currencyId = this.readonlyValueCurrencyId(entry);
    const currency = this.ws.get('Currency', currencyId) as Currency;
    return !!currency ? currency.E : this.ws.settings.FunctionalCurrencyDecimals;
  }

  public MonetaryValue_format(entry: Entry): string {
    const decimals = this.MonetaryValue_decimals(entry);
    return `1.${decimals}-${decimals}`;
  }

  // Entry Classification

  public showEntryType(entry: Entry): boolean {
    const account = this.account(entry);
    if (!account || !account.AccountTypeId) {
      return false;
    }

    // Show entry type when the account's type has an entry type parent Id
    const accountType = this.ws.get('AccountType', account.AccountTypeId) as AccountType;
    return !!accountType.EntryTypeParentId;
  }

  public readonlyEntryType(entry: Entry): boolean {
    const account = this.account(entry);
    return !!account && !!account.EntryTypeId;
  }

  public readonlyValueEntryTypeId(entry: Entry): number {
    const account = this.account(entry);
    return !!account ? account.EntryTypeId : null;
  }

  public filterEntryType(entry: Entry): string {
    const account = this.account(entry);
    if (!account || !account.AccountTypeId) {
      return null;
    }

    const accountType = this.ws.get('AccountType', account.AccountTypeId) as AccountType;
    return `IsAssignable eq true and IsActive eq true and Node descof ${accountType.EntryTypeParentId}`;
  }

  // External Reference

  public showExternalReference(entry: Entry): boolean {
    const account = this.account(entry);
    return !!account ? account.HasExternalReference : false;
  }

  // Additional Reference

  public showAdditionalReference(entry: Entry): boolean {
    const account = this.account(entry);
    return !!account ? account.HasAdditionalReference : false;
  }

  // Noted Agent Id

  public showNotedAgent(entry: Entry): boolean {
    const account = this.account(entry);
    return !!account ? account.HasNotedAgentId : false;
  }

  // Noted Agent Name

  public showNotedAgentName(entry: Entry): boolean {
    const account = this.account(entry);
    return !!account ? account.HasNotedAgentName : false;
  }

  // Noted Amount
  public showNotedAmount(entry: Entry): boolean {
    const account = this.account(entry);
    return !!account ? account.HasNotedAmount : false;
  }

  // Noted Date
  public showNotedDate(entry: Entry): boolean {
    const account = this.account(entry);
    return !!account ? account.HasNotedDate : false;
  }

  public onFileSelected(input: any, model: DocumentForSave) {
    const files = input.files as FileList;

    if (!files || files.length === 0) {
      return;
    }

    const file = files[0];
    input.value = '';
    if (file.size > this._maxAttachmentSize) {
      this.details.displayModalError(this.translate.instant('Error_FileSizeExceedsMaximumSizeOf0',
        {
          size: fileSizeDisplay(this._maxAttachmentSize)
        }));
      return;
    }

    // Make sure pending attachments don't exceed max file size
    model.Attachments = model.Attachments || [];
    const sumOfAttachmentSizesPendingSave = model.Attachments
      .map(a => !!a.file ? a.file.size : 0)
      .reduce((total, v) => total + v, 0);

    if (sumOfAttachmentSizesPendingSave + file.size > this._maxAttachmentSize) {
      this.details.displayModalError(this.translate.instant('Error_PendingFilesExceedMaximumSizeOf0',
        {
          size: fileSizeDisplay(this._maxAttachmentSize)
        }));
      return;
    }

    getDataURL(file).pipe(
      takeUntil(this.notifyDestruct$),
      tap(dataUrl => {

        // Get the base64 value from the data URL
        const commaIndex = dataUrl.indexOf(',');
        const fileBytes = dataUrl.substr(commaIndex + 1);
        const fileNamePieces = file.name.split('.');
        const extension = fileNamePieces.length > 1 ? fileNamePieces.pop() : null;
        const fileName = fileNamePieces.join('.') || '???';
        model.Attachments.push({
          Id: 0,
          File: fileBytes,
          FileName: fileName,
          FileExtension: extension,
          file,

          toJSON() {
            return {
              Id: this.Id,
              File: this.File,
              FileName: this.FileName,
              FileExtension: this.FileExtension
            };
          }
        });
      }),
      catchError(err => {
        console.error(err);
        return throwError(err);
      })
    ).subscribe();
  }

  public onDeleteAttachment(model: DocumentForSave, index: number) {
    model.Attachments.splice(index, 1);
  }

  public onDownloadAttachment(model: DocumentForSave, index: number) {
    const docId = model.Id;
    const att = model.Attachments[index];

    if (!!att.Id) {
      att.downloading = true; // show a little spinner
      this.documentsApi.getAttachment(docId, att.Id).pipe(
        tap(blob => {
          delete att.downloading;
          downloadBlob(blob, this.fileName(att));
        }),
        catchError(friendlyError => {
          delete att.downloading;
          this.details.displayModalError(friendlyError.error);
          return of(null);
        }),
        finalize(() => {
          delete att.downloading;
        })
      ).subscribe();

    } else if (!!att.file) {
      downloadBlob(att.file, this.fileName(att));
    }
  }

  private fileName(att: Attachment) {
    return !!att.FileName && !!att.FileExtension ? `${att.FileName}.${att.FileExtension}` :
      (att.FileName || (!!att.file ? att.file.name : 'Attachment'));
  }

  public size(att: Attachment): string {
    return fileSizeDisplay(att.Size || (!!att.file ? att.file.size : null));
  }

  public iconFromExtension(extension: string): string {
    if (!extension) {
      return 'file';
    } else {
      extension = extension.toLowerCase();
      switch (extension) {
        case 'pdf':
          return 'file-pdf';

        case 'doc':
        case 'docx':
          return 'file-word';

        case 'xls':
        case 'xlsx':
          return 'file-excel';

        case 'ppt':
        case 'pptx':
          return 'file-powerpoint';

        case 'txt':
        case 'rtf':
          return 'file-alt';

        case 'zip':
        case 'rar':
        case '7z':
        case 'tar':
          return 'file-archive';

        case 'jpg':
        case 'jpeg':
        case 'jpe':
        case 'jif':
        case 'jfif':
        case 'jfi':
        case 'png':
        case 'ico':
        case 'gif':
        case 'webp':
        case 'tiff':
        case 'tif':
        case 'psd':
        case 'raw':
        case 'arw':
        case 'cr2':
        case 'nrw':
        case 'k25':
        case 'bmp':
        case 'dib':
        case 'heif':
        case 'heic':
        case 'ind':
        case 'indd':
        case 'indt':
        case 'jp2':
        case 'j2k':
        case 'jpf':
        case 'jpx':
        case 'jpm':
        case 'mj2':
        case 'svg':
        case 'svgz':
        case 'ai':
        case 'eps':
          return 'file-image';

        case 'mpg':
        case 'mp2':
        case 'mpeg':
        case 'mpe':
        case 'mpv':
        case 'ogg':
        case 'mp4':
        case 'm4p':
        case 'm4v':
        case 'avi':
        case 'wmv':
        case 'mov':
        case 'qt':
        case 'flv':
        case 'swf':
          return 'file-video';

        case 'mp3':
        case 'aac':
        case 'wma':
        case 'flac':
        case 'alac':
        case 'wav':
        case 'aiff':
          return 'file-audio';

        default:
          return 'file';
      }
    }
  }

  public registerPristineFunc = (pristineDoc: DocumentForSave) => {
    const tracked = this.removeUntrackedProperties(pristineDoc);
    this._pristineDocJson = JSON.stringify(tracked);
  }

  public isDirtyFunc = (model: DocumentForSave) => {
    return this._pristineDocJson !== JSON.stringify(this.removeUntrackedProperties(model));
  }

  private removeUntrackedProperties(doc: Document): Document {
    if (!doc) {
      return doc;
    }

    // These properties and collections are not edited directly
    // and therefore need not be compared for dirty checking
    const copy = { ...doc } as Document;
    delete copy.AssignmentsHistory;
    if (!!doc.Attachments) {
      copy.Attachments = doc.Attachments.map(att => {
        const attCopy = { ...att } as Attachment;
        delete attCopy.file;
        delete attCopy.File;
        delete attCopy.downloading;
        return attCopy;
      });
    }

    return copy;
  }

  public get extraParams(): { [key: string]: any } {
    return { includeRequiredSignatures: true };
  }

  public handleFreshExtras(extras: { [key: string]: any }) {
    if (!!extras) {
      const relatedEntities = extras.RequiredSignaturesRelatedEntities as ({ [key: string]: EntityWithKey[] });
      if (!!relatedEntities) {
        mergeEntitiesInWorkspace(relatedEntities, this.workspace);
      }
    }
  }
  public requiredSignaturesForLineDef(
    model: Document, lineDef: string, extras: { [key: string]: any }): RequiredSignature[] {
    if (this._requiredSignaturesForLineDefModel !== model ||
      this._requiredSignaturesForLineDefLineDef !== lineDef) {
      this._requiredSignaturesForLineDefModel = model;
      this._requiredSignaturesForLineDefLineDef = lineDef;
      this._requiredSignaturesForLineDefLineIds = model.Lines
        .filter(l => !!l.Id && l.DefinitionId === lineDef)
        .map(l => l.Id as number);
    }

    return this.requiredSignaturesSummaryInner(this._requiredSignaturesForLineDefLineIds, extras);
  }

  public requiredSignaturesSummaryInner(
    lineIds: number[],
    extras: { [key: string]: any }
  ): RequiredSignature[] {

    if (!lineIds || !extras || !extras.RequiredSignatures) {
      return [];
    }

    // This function implements a "group by" algorithm of required signatures
    const requiredSignaturesDetailed = extras.RequiredSignatures as RequiredSignature[];
    if (requiredSignaturesDetailed !== this._requiredSignaturesDetailed || lineIds !== this._requiredSignaturesLineIds) {
      this._requiredSignaturesDetailed = requiredSignaturesDetailed;
      this._requiredSignaturesLineIds = lineIds;

      // Put all included line IDs in a hash table for quick lookup
      const includedLineIds: { [id: number]: true } = {};
      for (const lineId of lineIds) {
        includedLineIds[lineId] = true;
      }

      const lineIdsHash: HashTable = {};
      const result: RequiredSignature[] = [];
      for (const signature of requiredSignaturesDetailed.filter(e => includedLineIds[e.LineId])) {
        let currentHash = lineIdsHash;
        let newGroup = false;
        for (const prop of this._requiredSignatureProps) {
          const value = signature[prop];

          if (value === null || value === undefined) {
            // for null or undefined values, use the "undefined" property
            if (!currentHash.undefined) {
              currentHash.undefined = {};
              newGroup = true;
            }

            currentHash = currentHash.undefined;
          } else {
            // for defined values, use the "values" property
            if (!currentHash.values) {
              currentHash.values = {};
            }

            if (currentHash.values[value] === undefined) {
              currentHash.values[value] = {};
              newGroup = true;
            }

            currentHash = currentHash.values[value];
          }
        }

        if (!currentHash.lineIds) {
          currentHash.lineIds = [signature.LineId];
        } else {
          currentHash.lineIds.push(signature.LineId);
        }

        if (newGroup) {
          // The signature clone will represent this group
          const clone = { ...signature } as RequiredSignature;
          delete clone.LineId;
          result.push(clone);
        }
      }

      this._requiredSignaturesLineIdsHash = lineIdsHash;
      this._requiredSignaturesSummary = result.sort((a, b) => {
        // Order by to state
        let diff = Math.abs(a.ToState) - Math.abs(b.ToState);

        // Then by rule type
        if (diff === 0) {
          const aRT = a.RuleType || '0';
          const bRT = b.RuleType || '0';
          diff = aRT > bRT ? 1 : aRT < bRT ? -1 : 0;
        }

        // Then by role ID
        if (diff === 0) {
          diff = (a.RoleId || 0) - (b.RoleId || 0);
        }

        // Return result
        return diff;
      });
    }

    return this._requiredSignaturesSummary;
  }

  public lineIds(requiredSignature: RequiredSignature): number[] {
    if (!requiredSignature) {
      return [];
    }

    let currentHash: HashTable = this._requiredSignaturesLineIdsHash;
    for (const prop of this._requiredSignatureProps) {
      const value = requiredSignature[prop];
      if (value === null || value === undefined) {
        currentHash = currentHash.undefined;
      } else {
        currentHash = currentHash.values[value];
      }
    }

    return currentHash.lineIds;
  }

  public onSignYes(signature: RequiredSignature): void {
    this.onSign(signature, true);
  }

  public onSignNo(signature: RequiredSignature): void {
    this.onSign(signature, false);
  }

  private onSign(signature: RequiredSignature, yes: boolean): void {
    const lineIds = this.lineIds(signature);
    this.documentsApi.sign(lineIds, {
      returnEntities: true,
      expand: this.expand,
      select: undefined,
      onBehalfOfUserId: signature.OnBehalfOfUserId,
      toState: yes ? signature.ToState : -signature.ToState,
      roleId: signature.RoleId,
      ruleType: signature.RuleType,
      reasonDetails: null,
      reasonId: null,
      signedAt: null,
    }, { includeRequiredSignatures: true }).pipe(
      tap(res => {
        addToWorkspace(res, this.workspace);
        this.details.state.extras = res.Extras;
        this.handleFreshExtras(res.Extras);
      }),
      catchError(friendlyError => {
        this.details.handleActionError(friendlyError); return of(null);
      })
    ).subscribe();
  }

  public onUnsign(signature: RequiredSignature) {
    this.confirmationMessage = this.translate.instant('AreYouSureYouWantToDeleteYourSignature');
    const modalRef = this.modalService.open(this.confirmModal);
    modalRef.result.then(
      (confirmed: boolean) => {
        if (confirmed) {
          this.documentsApi.unsign(this.lineIds(signature), {
            returnEntities: true,
            expand: this.expand,
            select: undefined
          }, { includeRequiredSignatures: true }).pipe(
            tap(res => {
              addToWorkspace(res, this.workspace);
              this.details.state.extras = res.Extras;
              this.handleFreshExtras(res.Extras);
            }),
            catchError(friendlyError => {
              this.details.handleActionError(friendlyError);
              return of(null);
            })
          ).subscribe();
        }
      },
      _ => { }
    );
  }

  public canUnsign(signature: RequiredSignature) {
    return !!signature.SignedById && signature.SignedById === this.ws.userSettings.UserId;
  }

  public showState(model: Document, requiredSignatures: RequiredSignature[], state: number) {
    if (!model || !model.Id) {
      return false;
    }

    if (model.State === state) {
      return true;
    }

    return !!requiredSignatures && requiredSignatures.some(e => Math.abs(e.ToState) === state);
  }

  public positiveActionDisplay(toState: number): string {
    // Used for button
    return this.actionDisplay(Math.abs(toState));
  }

  public positiveActionIcon(toState: number): string {
    // Used for button
    return this.actionIcon(Math.abs(toState));
  }

  public negativeActionDisplay(toState: number): string {
    // Used for button
    return this.actionDisplay(-Math.abs(toState));
  }

  public negativeActionIcon(toState: number): string {
    // Used for button
    return this.actionIcon(-Math.abs(toState));
  }

  private actionIcon(toState: number): string {
    // Used for stamp
    switch (toState) {
      case 1: return 'arrow-right';
      case 2: return 'thumbs-up';
      case 3: return 'check';
      case 4: return 'check';

      case -1: return 'times';
      case -2: return 'thumbs-down';
      case -3: return 'times';
      case -4: return 'times';
      default: return '';
    }
  }

  public actionDisplay(toState: number): string {
    // Used for stamp
    switch (toState) {
      case 1: return this.translate.instant('Document_State_Requested');
      case 2: return this.translate.instant('Document_State_Authorized');
      case 3: return this.translate.instant('Document_State_Completed');
      case 4: return this.translate.instant('Document_State_Reviewed');

      case -1: return this.translate.instant('Document_State_Void');
      case -2: return this.translate.instant('Document_State_Rejected');
      case -3: return this.translate.instant('Document_State_Failed');
      case -4: return this.translate.instant('Document_State_Invalid');
      default: return '';
    }
  }

  public requiredSignatureDisplay(signature: RequiredSignature) {
    // Used for the footer of the stamp in all rule types except 'Public'
    switch (Math.abs(signature.ToState)) {
      case 1: return this.translate.instant('RequestedBy');
      case 2: return this.translate.instant('AuthorizedBy');
      case 3: return this.translate.instant('CompletedBy');
      case 4: return this.translate.instant('ReviewedBy');
    }
  }

  public requiredSignatoryDisplay(signature: RequiredSignature) {
    // Used for the footer of the stamp for rule type 'Public'
    switch (Math.abs(signature.ToState)) {
      case 1: return this.translate.instant('Requester');
      case 2: return this.translate.instant('Authorizer');
      case 3: return this.translate.instant('Completer');
      case 4: return this.translate.instant('Reviewer');
    }
  }
}

/**
 * Hashes one dimension of an aggregate result for the pivot table
 */
interface HashTable {
  values?: { [value: string]: HashTable };
  undefined?: HashTable;

  lineIds?: number[];
}
