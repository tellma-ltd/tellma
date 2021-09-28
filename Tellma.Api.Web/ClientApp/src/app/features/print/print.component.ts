// tslint:disable:member-ordering
import { Component, ElementRef, HostBinding, Input, OnChanges, OnDestroy, OnInit, SimpleChanges, TemplateRef, ViewChild } from '@angular/core';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { ApiService } from '~/app/data/api.service';
import { PrintingTemplateForClient } from '~/app/data/dto/definitions-for-client';
import { PrintStore, ReportArguments, ReportStatus, WorkspaceService } from '~/app/data/workspace.service';
import { Subject, Observable, of, Subscription, merge } from 'rxjs';
import { tap, catchError, switchMap, debounceTime, map } from 'rxjs/operators';
import { metadata, PropVisualDescriptor } from '~/app/data/entities/base/metadata';
import { descFromControlOptions, downloadBlob, fileSizeDisplay, FriendlyError, printBlob } from '~/app/data/util';
import { PrintArguments, PrintEntitiesArguments, PrintEntityByIdArguments } from '~/app/data/dto/print-arguments';
import { PrintPreviewResponse } from '~/app/data/dto/printing-preview-response';
import { PrintingPreviewTemplate } from '~/app/data/dto/printing-preview-template';

export interface PrintingTemplates {
  context?: string;
  downloadName?: string;
  body?: string;
}

@Component({
  selector: 't-print',
  templateUrl: './print.component.html',
  styles: [
  ]
})
export class PrintComponent implements OnInit, OnChanges, OnDestroy {

  private _subscriptions: Subscription;
  private printingTemplatesApi = this.api.printingTemplatesApi(null); // for intellisense

  @ViewChild('iframe')
  iframe: ElementRef<HTMLIFrameElement>;

  @ViewChild('errorModal', { static: true })
  public errorModal: TemplateRef<any>;

  @Input()
  template: PrintingTemplateForClient; // Used in preview mode

  @Input()
  preview: PrintingTemplates;

  @HostBinding('class.h-100')
  h100 = true;

  private url: string; // For revoking

  private notifyDestruct$ = new Subject<void>();
  private notifyFetch$ = new Subject<void>();
  private notifyDelayedFetch$ = new Subject<void>();

  constructor(
    private workspace: WorkspaceService,
    private api: ApiService,
    private router: Router,
    private route: ActivatedRoute,
    private translate: TranslateService) {
  }

  ngOnInit() {
    this._subscriptions = new Subscription();

    this.printingTemplatesApi = this.api.printingTemplatesApi(this.notifyDestruct$); // for intellisense

    // Hook the fetch signals
    const templateSignals = this.notifyDelayedFetch$.pipe(
      debounceTime(300),
    );

    const otherSignals = this.notifyFetch$;
    const allSignals = merge(templateSignals, otherSignals);

    this._subscriptions.add(allSignals.pipe(
      switchMap(_ => this.doFetch())
    ).subscribe());

    const onUrlChange = (params: ParamMap) => {
      // This triggers changes on the screen
      if (this.isScreenMode) {

        const templateId = +params.get('templateId');

        let template: PrintingTemplateForClient;
        if (!!templateId) {
          template = this.workspace.currentTenant.definitions.PrintingTemplates.find(e => e.PrintingTemplateId === templateId);

          if (!template) {
            this.router.navigate(['page-not-found'], { relativeTo: this.route.parent, replaceUrl: true });
          }
        }

        this.template = template;

        // Read the arguments from the URL
        // for (const p of this.parameters) {
        //   let urlValue: any;
        //   if (params.has(p.keyLower)) {
        //     const urlStringValue = params.get(p.keyLower);
        //     try {
        //       switch (p.datatype) {
        //         case 'datetimeoffset':
        //           const dto = new Date(urlStringValue);
        //           if (!isNaN(dto.getTime())) {
        //             if (/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{7}Z$/.test(urlStringValue)) {
        //               urlValue = urlStringValue;
        //             } else {
        //               urlValue = dto.toISOString().replace('Z', '0000Z');
        //             }
        //           }
        //           break;
        //         case 'datetime':
        //         case 'date':
        //           const date = new Date(urlStringValue);
        //           if (!isNaN(date.getTime())) {
        //             urlValue = toLocalDateTimeISOString(date);
        //           }
        //           break;
        //         case 'string':
        //           urlValue = urlStringValue;
        //           break;
        //         case 'numeric':
        //           urlValue = +urlStringValue;
        //           break;
        //         case 'bit':
        //           urlValue = urlStringValue.toLowerCase() === 'true';
        //           break;
        //         case 'boolean':
        //         case 'hierarchyid':
        //         case 'geography':
        //         case 'entity':
        //         case 'null':
        //         default:
        //           console.error(`Unsupported parameter datatype ${p.datatype}.`);
        //           break;
        //       }
        //     } catch (ex) {
        //       console.error(ex);
        //     }
        //   }

        //   this.arguments[p.keyLower] = urlValue;
        // }
      }
    };

    // Pick up state from the URL
    this._subscriptions.add(this.route.paramMap.subscribe(onUrlChange));
    onUrlChange(this.route.snapshot.paramMap);

    this.fetch();
  }

  ngOnDestroy() {
    window.URL.revokeObjectURL(this.url);
    this._subscriptions.unsubscribe();
    this.notifyDestruct$.next();
  }

  ngOnChanges(changes: SimpleChanges) {
    const templateChange = changes.template;
    if (!!templateChange && !templateChange.isFirstChange()) {
      const old = templateChange.previousValue as PrintingTemplateForClient;
      const curr = templateChange.currentValue as PrintingTemplateForClient;

      if (!!old && !!curr) {
        if (old.Collection !== curr.Collection ||
          old.DefinitionId !== curr.DefinitionId) {
          this.resetState();
        }
      }

      this.fetch();
      return; // No point checking the rest
    }

    const previewChange = changes.preview;
    if (!!previewChange && !previewChange.isFirstChange()) {
      const old = previewChange.previousValue as PrintingTemplates;
      const curr = previewChange.currentValue as PrintingTemplates;

      if (!!old && !!curr) {
        if (old.context !== curr.context || old.downloadName !== curr.downloadName) {
          this.fetch();
        } else {
          this.delayedFetch(); // Body changes
        }
      }
    }
  }

  private resetState(): void {
    // Reset state in workspace
    const s = this.state;
    s.top = 25;
    s.skip = 0;
    s.filter = undefined;
    s.orderby = undefined;
    s.id = undefined;
    s.lang = 1;

    // this.details.urlStateChange();

    // reset state of the screen
    s.blob = undefined;
    window.URL.revokeObjectURL(this.url);
    this.url = undefined;

    if (!!this.iframe) {
      this.iframe.nativeElement.contentWindow.location.replace('about:blank');
    }

    s.reportStatus = undefined;
    s.fileSizeDisplay = undefined;
    s.errorMessage = undefined;
    s.information = undefined;
  }

  get isScreenMode(): boolean {
    return !this.preview;
  }

  public get state(): PrintStore {

    const key = this.stateKey;
    const rs = this.workspace.currentTenant.printState;
    if (!rs[key]) {
      rs[key] = new PrintStore();
    }

    return rs[key];
  }

  public get stateKey(): string {
    return this.isScreenMode ? this.template.PrintingTemplateId.toString() : `preview`;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  private delayedFetch(): void {
    this.notifyDelayedFetch$.next();
  }

  private fetch(): void {
    this.notifyFetch$.next();
  }

  private doFetch(): Observable<void> {
    const settings = this.ws.settings;
    const s = this.state;
    const template = this.template;

    s.errorMessage = undefined;
    s.information = undefined;

    if (!template) {
      s.reportStatus = ReportStatus.information;
      return of();
    }

    // Use a sensible culture value
    const defaultLang = template.SupportsPrimaryLanguage ? settings.PrimaryLanguageId :
      template.SupportsSecondaryLanguage ? settings.SecondaryLanguageId :
        template.SupportsTernaryLanguage ? settings.TernaryLanguageId : settings.PrimaryLanguageId;
    const culture =
      s.lang === 1 && template.SupportsPrimaryLanguage ? settings.PrimaryLanguageId :
        s.lang === 2 && template.SupportsSecondaryLanguage ? settings.SecondaryLanguageId :
          s.lang === 3 && template.SupportsTernaryLanguage ? settings.TernaryLanguageId : defaultLang;


    let obs$: Observable<{ blob: Blob, name: string }>;

    if (this.isScreenMode) {
      if (template.Usage === 'Standalone' && !!template.PrintingTemplateId) {
        const args: PrintArguments = {
          culture
        };

        obs$ = this.printingTemplatesApi.print(template.PrintingTemplateId, args);
      } else {
        console.error('Using a non standalone printing template as standalone');
        this.router.navigate(['page-not-found'], { relativeTo: this.route.parent, replaceUrl: true });
      }
    } else {
      // Preview

      const previewEntity: PrintingPreviewTemplate = {
        Collection: template.Collection,
        DefinitionId: template.DefinitionId,
        Context: this.preview.context,
        DownloadName: this.preview.downloadName,
        Body: this.preview.body
      };

      let previewObs$: Observable<PrintPreviewResponse>;
      if (template.Usage === 'FromDetails') {
        if (!template.Collection) {
          s.information = () => `Please specify the collection in Metadata.`;
          s.reportStatus = ReportStatus.information;
          return of();
        }

        const defIdRequired = !!template.Collection && !!metadata[template.Collection](this.workspace, this.translate, null).definitionIds;
        if (defIdRequired && !template.DefinitionId) {
          s.information = () => 'Please specify the definition in Metadata.';
          s.reportStatus = ReportStatus.information;
          return of();
        }

        if (!this.id) {
          s.information = () => `Please specify the ${this.detailsPickerLabel} above`;
          s.reportStatus = ReportStatus.information;
          return of();
        }

        const args: PrintEntityByIdArguments = {
          culture,
        };

        previewObs$ = this.printingTemplatesApi.previewById(this.id, previewEntity, args);
      } else if (template.Usage === 'FromSearchAndDetails') {
        if (!template.Collection) {
          s.information = () => 'Please specify the definition in Metadata.';
          s.reportStatus = ReportStatus.information;
          return of();
        }

        const args: PrintEntitiesArguments = {
          culture,
          filter: this.filter,
          orderby: this.orderby,
          top: this.top,
          skip: this.skip
        };

        previewObs$ = this.printingTemplatesApi.previewByFilter(previewEntity, args);
      } else {
        const args: PrintArguments = {
          culture
        };

        previewObs$ = this.printingTemplatesApi.preview(previewEntity, args);
      }

      obs$ = previewObs$.pipe(
        map(res => ({ blob: new Blob([res.Body], { type: 'text/html' }), name: res.DownloadName }))
      );
    }

    s.reportStatus = ReportStatus.loading;
    return obs$.pipe(
      tap(pair => {
        const blob = pair.blob;
        s.fileDownloadName = pair.name;
        s.blob = blob;

        window.URL.revokeObjectURL(this.url);
        this.url = window.URL.createObjectURL(blob) + '#toolbar=0&navpanes=0&scrollbar=0';

        // We just made it, so it's definitely safe
        this.iframe.nativeElement.contentWindow.location.replace(this.url);
        s.fileSizeDisplay = fileSizeDisplay(blob.size);
        s.reportStatus = ReportStatus.loaded;
      }),
      catchError((friendlyError: FriendlyError) => {
        if (friendlyError instanceof TypeError) {
          console.error(friendlyError);
        }

        s.errorMessage = friendlyError.error || 'Unknown error.';
        s.reportStatus = ReportStatus.error;
        return of(null);
      })
    );
  }

  // UI Binding

  public get showTitle(): boolean {
    return this.template.Usage === 'Standalone' && !!this.title;
  }

  public get title(): string {
    const template = this.template;
    return this.ws.localize(template.Name, template.Name2, template.Name3);
  }

  public get showParametersSection(): boolean {
    return this.showMasterAndDetailsParams || this.showDetailsParams; // TODO
  }

  public get showMasterAndDetailsParams() {
    const template = this.template;
    return !!template && template.Usage === 'FromSearchAndDetails' && !!template.Collection;
  }

  public get showDetailsParams() {
    const template = this.template;
    return !!template && template.Usage === 'FromDetails' && !!template.Collection;
  }

  public get detailsPickerLabel(): string {
    const template = this.template;
    if (!!template && !!template.Collection) {
      const descFunc = metadata[template.Collection];
      const desc = descFunc(this.workspace, this.translate, template.DefinitionId);
      return desc.titleSingular();
    }

    return ''; // Should not reach here in theory
  }

  private _templateForDesc: PrintingTemplateForClient;
  private _detailsPickerDesc: PropVisualDescriptor;
  public get detailsPickerDesc(): PropVisualDescriptor {
    const template = this.template;
    if (this._templateForDesc !== template) {
      this._templateForDesc = template;

      if (!!template && !!template.Collection) {
        let options: string = null;
        if (!!template.DefinitionId) {
          options = JSON.stringify({ definitionId: template.DefinitionId });
        }
        console.log(options);
        this._detailsPickerDesc = descFromControlOptions(this.ws, template.Collection, options);
      } else {
        this._detailsPickerDesc = null;
      }
    }

    return this._detailsPickerDesc;
  }

  public onParameterChange() {
    // this.details.urlStateChange();
    this.fetch();
  }

  public get filter(): string {
    return this.state.filter;
  }

  public set filter(v: string) {
    this.state.filter = v;
  }

  public get orderby(): string {
    return this.state.orderby;
  }

  public set orderby(v: string) {
    this.state.orderby = v;
  }

  public get top(): number {
    return this.state.top;
  }

  public set top(v: number) {
    this.state.top = v;
  }

  public get skip(): number {
    return this.state.skip;
  }

  public set skip(v: number) {
    this.state.skip = v;
  }

  public get id(): number | string {
    return this.state.id;
  }

  public set id(v: number | string) {
    this.state.id = v;
  }

  public get lang(): 1 | 2 | 3 {
    return this.state.lang;
  }

  public set lang(v: 1 | 2 | 3) {
    this.state.lang = v;
  }

  public get fileDownloadName(): string {
    return this.state.fileDownloadName;
  }

  public get fileSizeDisplay(): string {
    return this.state.fileSizeDisplay;
  }

  // Command Bar

  onPrint() {
    const s = this.state;
    printBlob(s.blob, s.fileDownloadName);
  }

  public get disablePrint(): boolean {
    return !this.state.blob; // this.loading || !!this.error || !this.safeUrl;
  }

  public onDownload() {
    const s = this.state;
    if (!!s.blob) {
      downloadBlob(s.blob, s.fileDownloadName);
    }
  }

  public get disableDownload(): boolean {
    return !this.state.blob; // this.loading || !!this.error || !this.safeUrl;
  }

  onRefresh() {
    if (this.state.reportStatus !== ReportStatus.loading) {
      this.fetch();
    }
  }

  public get showSpinner(): boolean {
    return this.state.reportStatus === ReportStatus.loading;
  }

  public showLanguageToggle(): boolean {
    return (this.showLang(1) ? 1 : 0) +
      (this.showLang(2) ? 1 : 0) +
      (this.showLang(3) ? 1 : 0) > 1;
  }

  public langDisplay(lang: 1 | 2 | 3): string {
    if (lang === 1) {
      return this.ws.settings.PrimaryLanguageSymbol;
    }
    if (lang === 2) {
      return this.ws.settings.SecondaryLanguageSymbol;
    }
    if (lang === 3) {
      return this.ws.settings.TernaryLanguageSymbol;
    }

    return '';
  }

  public onLang(lang: 1 | 2 | 3): void {
    if (this.lang !== lang) {
      this.lang = lang;
      // this.details.urlStateChange();
      this.fetch();
    }
  }

  public isLang(lang: 1 | 2 | 3): boolean {
    return this.lang === lang;
  }

  public showLang(lang: 1 | 2 | 3): boolean {
    const template = this.template;
    return (lang === 1 && !!template.SupportsPrimaryLanguage) ||
      (lang === 2 && !!template.SupportsSecondaryLanguage && !!this.ws.settings.SecondaryLanguageId) ||
      (lang === 3 && !!template.SupportsTernaryLanguage && !!this.ws.settings.TernaryLanguageId);
  }

  public get showInfo(): boolean {
    return this.state.reportStatus === ReportStatus.information;
  }

  public get message(): string {
    return this.state.information();
  }

  public get error(): string {
    return this.state.errorMessage;
  }
}
