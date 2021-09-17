// tslint:disable:member-ordering
import { Component, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { ApiService } from '~/app/data/api.service';
import { WorkspaceService, MasterDetailsStore } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { TranslateService } from '@ngx-translate/core';
import { ChoicePropDescriptor, collectionsWithEndpoint, getChoices, metadata } from '~/app/data/entities/base/metadata';
import { SelectorChoice } from '~/app/shared/selector/selector.component';
import { PrintingTemplateForSave, metadata_PrintingTemplate, PrintingTemplate } from '~/app/data/entities/printing-template';
import { NgControl } from '@angular/forms';
import { validationErrors, highlightInvalid, areServerErrors } from '~/app/shared/form-group-base/form-group-base.component';
import { Subject, Observable, of, Subscription, merge } from 'rxjs';
import { tap, catchError, switchMap, debounceTime } from 'rxjs/operators';
import { fileSizeDisplay, FriendlyError, printBlob, onCodeTextareaKeydown, downloadBlob } from '~/app/data/util';
import {
  PrintEntitiesArguments, PrintEntityByIdArguments, PrintArguments
} from '~/app/data/dto/print-arguments';
import { PrintPreviewResponse } from '~/app/data/dto/printing-preview-response';

@Component({
  selector: 't-printing-templates-details',
  templateUrl: './printing-templates-details.component.html',
  styles: []
})
export class PrintingTemplatesDetailsComponent extends DetailsBaseComponent implements OnInit, OnDestroy {

  private notifyFetch$ = new Subject<PrintingTemplateForSave>();
  private templateChanged$ = new Subject<PrintingTemplateForSave>();
  private printingTemplatesApi = this.api.printingTemplatesApi(this.notifyDestruct$); // for intellisense
  private localState = new MasterDetailsStore();  // Used in popup mode

  @ViewChild('iframe')
  iframe: ElementRef;

  private _sections: { [key: string]: boolean } = {
    Metadata: false,
    Template: true
  };

  public expand = '';
  public collapseEditor = false;
  public collapseMetadata = true;

  public fileDownloadName: string; // For downloading
  public blob: Blob; // For downloading/printing
  public url: string; // For revoking
  public fileSizeDisplay: string;
  public error: string;
  public message: string;
  public loading = false;

  create = () => {
    const result: PrintingTemplateForSave = {};
    if (this.ws.isPrimaryLanguage) {
      result.Name = this.initialText;
    } else if (this.ws.isSecondaryLanguage) {
      result.Name2 = this.initialText;
    } else if (this.ws.isTernaryLanguage) {
      result.Name3 = this.initialText;
    }

    result.IsDeployed = false;
    result.SupportsPrimaryLanguage = true;
    result.SupportsSecondaryLanguage = !!this.workspace.currentTenant.settings.SecondaryLanguageId;
    result.SupportsTernaryLanguage = !!this.workspace.currentTenant.settings.TernaryLanguageId;

    result.Usage = 'FromMasterAndDetails';
    result.Collection = 'Document';
    result.Body = defaultBody;

    return result;
  }

  constructor(private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();

    this.printingTemplatesApi = this.api.printingTemplatesApi(this.notifyDestruct$);
  }

  ngOnInit() {
    super.ngOnInit();

    // const handleFreshStateFromUrl = (params: ParamMap) => {
    //   if (this.isScreenMode) {
    //     // Grab the state
    //     const s = this.state.detailsState;

    //     // When set to true, it means the url is out of step with the state
    //     let triggerUrlStateChange = false;
    //     let triggerRefresh = false;

    //     // filter
    //     const urlFilter = params.get('filter');
    //     if (!!urlFilter) {
    //       if (s.filter !== urlFilter) {
    //         s.filter = urlFilter;
    //         triggerRefresh = true;
    //       }
    //     } else if (!!s.filter) { // Prevents infinite loop
    //       triggerUrlStateChange = true;
    //     }

    //     // orderby
    //     const urlOrderBy = params.get('orderby');
    //     if (!!urlOrderBy) {
    //       if (s.orderby !== urlOrderBy) {
    //         s.orderby = urlOrderBy;
    //         triggerRefresh = true;
    //       }
    //     } else if (!!s.orderby) { // Prevents infinite loop
    //       triggerUrlStateChange = true;
    //     }

    //     // top
    //     const urlTop = params.get('top'); // default
    //     if (isSpecified(urlTop)) {
    //       const urlTopNumber = +urlTop;
    //       if (!!urlTopNumber) {
    //         if (s.top !== urlTopNumber) {
    //           s.top = urlTopNumber;
    //           triggerRefresh = true;
    //         }
    //       }
    //     } else {
    //       if (!isSpecified(s.top)) {
    //         s.top = 25; // Prevents infinite loop
    //       }
    //       triggerUrlStateChange = true;
    //     }

    //     // skip
    //     const urlSkip = params.get('skip'); // Default
    //     if (isSpecified(urlSkip)) {
    //       const urlSkipNumber = +urlSkip;
    //       if (!!urlSkipNumber) {
    //         if (s.skip !== urlSkipNumber) {
    //           s.skip = urlSkipNumber;
    //           triggerRefresh = true;
    //         }
    //       }
    //     } else { // Prevents infinite loop
    //       if (!isSpecified(s.skip)) {
    //         s.skip = 0;
    //       }
    //       triggerUrlStateChange = true;
    //     }

    //     // id
    //     const urlId = params.get('paramId');
    //     if (!!urlId) {
    //       if (s.id !== urlId) {
    //         s.id = urlId;
    //         triggerRefresh = true;
    //       }
    //     } else if (!!s.id) { // Prevents infinite loop
    //       triggerUrlStateChange = true;
    //     }

    //     const lang = params.get('lang');
    //     if (!!lang) {
    //       const langNumber = +lang;
    //       if (langNumber === 1 || langNumber === 2 || langNumber === 3) {
    //         s.lang = langNumber;
    //         triggerRefresh = true;
    //       }
    //     } else if (s.lang) {
    //       triggerUrlStateChange = true;
    //     }

    //     // The URL is out of step with the state => sync the two
    //     // This happens when we navigate to the screen again 2nd time
    //     if (triggerUrlStateChange && !!this.details) {
    //       // We must be careful here to avoid an infinite loop
    //       // this.details.urlStateChange();
    //     }

    //     if (triggerRefresh) {
    //       this.state.detailsState.modelId = null;
    //     }
    //   }
    // };

    // this._subscriptions.add(this.route.paramMap.pipe(skip(1)).subscribe(handleFreshStateFromUrl)); // future changes
    // handleFreshStateFromUrl(this.route.snapshot.paramMap); // right now

    // Hook the fetch signals
    this._subscriptions = new Subscription();
    const templateSignals = this.templateChanged$.pipe(
      debounceTime(300),
    );

    const otherSignals = this.notifyFetch$;
    const allSignals = merge(templateSignals, otherSignals);

    this._subscriptions.add(allSignals.pipe(
      switchMap((template) => this.doFetch(template))
    ).subscribe());
  }

  ngOnDestroy() {
    super.ngOnDestroy();

    if (!!this.url) {
      window.URL.revokeObjectURL(this.url);
    }
  }

  public get state(): MasterDetailsStore {
    // important to always reference the source, and not keep a local reference
    // on some occasions the source can be reset and using a local reference can cause bugs
    if (this.isPopupMode) {

      // popups use a local store that vanishes when the popup is destroyed
      if (!this.localState) {
        this.localState = new MasterDetailsStore();
      }

      return this.localState;
    } else {

      // screen mode on the other hand use the global state
      return this.globalState;
    }
  }

  private get globalState(): MasterDetailsStore {
    const key = 'printing-templates';
    if (!this.workspace.current.mdState[key]) {
      this.workspace.current.mdState[key] = new MasterDetailsStore();
    }

    return this.workspace.current.mdState[key];
  }

  // /**
  //  * Encodes any custom screen state in the url params
  //  */
  // public encodeCustomStateFunc: (params: Params) => void = (params: Params) => {

  //   console.log('encodeCustomState');

  //   if (!!this.filter) {
  //     params.filter = this.filter;
  //   }
  //   if (!!this.orderby) {
  //     params.orderby = this.orderby;
  //   }
  //   if (!!this.top) {
  //     params.top = this.top;
  //   }
  //   if (!!this.skip) {
  //     params.skip = this.skip;
  //   }
  //   if (!!this.id) {
  //     params.paramId = this.id;
  //   }
  //   if (!!this.lang) {
  //     params.lang = this.lang;
  //   }
  // }

  public get ws() {
    return this.workspace.currentTenant;
  }

  // UI Binding

  public isInactive: (model: PrintingTemplate) => string = (_: PrintingTemplate) => null;

  public onParameterChange(model: PrintingTemplateForSave) {
    // this.details.urlStateChange();
    this.fetch(model);
  }

  public onDefinitionChange(model: PrintingTemplateForSave) {
    this.fetch(model);
  }

  public onCollectionChange(model: PrintingTemplateForSave) {
    this.id = null;
    // this.details.urlStateChange();
    model.DefinitionId = null;
    this.onDefinitionChange(model);
  }

  public onDefinitionIdChange(model: PrintingTemplateForSave) {
    this.id = null;
    // this.details.urlStateChange();
    this.onDefinitionChange(model);
  }

  private resetState() {
    // Reset state in workspace
    this.top = 25;
    this.skip = 0;
    this.filter = undefined;
    this.orderby = undefined;
    this.id = undefined;
    this.lang = 1;

    // this.details.urlStateChange();

    // reset state of the screen
    this.blob = undefined;
    if (!!this.url) {
      window.URL.revokeObjectURL(this.url);
    }
    this.url = undefined;
    if (!!this.iframe) {
      (this.iframe.nativeElement as HTMLIFrameElement).contentWindow.location.replace('about:blank');
    }
    this.fileSizeDisplay = undefined;
    this.error = undefined;
    this.message = undefined;
    this.loading = false;
  }

  public onBodyChange(value: string, model: PrintingTemplateForSave) {
    if (model.Body !== value) {
      model.Body = value;
      this.templateChanged$.next(model);
    }
  }

  public invalid(control: NgControl, serverErrors: string[]): boolean {
    return highlightInvalid(control, serverErrors);
  }

  public errors(control: NgControl, serverErrors: string[]): (() => string)[] {
    return validationErrors(control, serverErrors, this.translate);
  }

  public onToggleEditor(): void {
    this.collapseEditor = !this.collapseEditor;
  }

  public onToggleMetadata(): void {
    this.collapseMetadata = !this.collapseMetadata;
  }

  public flipIcon(isExpanded: boolean): string {
    return this.workspace.ws.isRtl && !isExpanded ? 'horizontal' : null;
  }

  public rotateIcon(isExpanded: boolean): number {
    return isExpanded ? 90 : 0;
  }

  public onToggleSection(key: string): void {
    this._sections[key] = !this._sections[key];
  }

  showSection(key: string): boolean {
    return this._sections[key];
  }

  public metadataPaneErrors(model: PrintingTemplate) {
    return !!model.serverErrors && (
      areServerErrors(model.serverErrors.Name) ||
      areServerErrors(model.serverErrors.Name2) ||
      areServerErrors(model.serverErrors.Name3) ||
      areServerErrors(model.serverErrors.Code) ||
      areServerErrors(model.serverErrors.Description) ||
      areServerErrors(model.serverErrors.Description2) ||
      areServerErrors(model.serverErrors.Description3) ||
      areServerErrors(model.serverErrors.Usage) ||
      areServerErrors(model.serverErrors.Collection) ||
      areServerErrors(model.serverErrors.DefinitionId) ||
      areServerErrors(model.serverErrors.DownloadName) ||
      areServerErrors(model.serverErrors.SupportsPrimaryLanguage) ||
      areServerErrors(model.serverErrors.SupportsSecondaryLanguage) ||
      areServerErrors(model.serverErrors.SupportsTernaryLanguage)
    );
  }

  public templateSectionErrors(model: PrintingTemplate) {
    return !!model.serverErrors && areServerErrors(model.serverErrors.Body);
  }

  private fetch(template: PrintingTemplateForSave) {
    this.notifyFetch$.next(template);
  }

  private doFetch(template: PrintingTemplateForSave): Observable<void> {
    const settings = this.ws.settings;

    // Use a sensible culture value
    const defaultLang = template.SupportsPrimaryLanguage ? settings.PrimaryLanguageId :
      template.SupportsSecondaryLanguage ? settings.SecondaryLanguageId :
        template.SupportsTernaryLanguage ? settings.TernaryLanguageId : settings.PrimaryLanguageId;
    const culture =
      this.lang === 1 && template.SupportsPrimaryLanguage ? settings.PrimaryLanguageId :
        this.lang === 2 && template.SupportsSecondaryLanguage ? settings.SecondaryLanguageId :
          this.lang === 3 && template.SupportsTernaryLanguage ? settings.TernaryLanguageId : defaultLang;

    this.error = undefined;
    this.message = undefined;

    if (!template) {
      this.loading = false;
      return of();
    }

    let obs$: Observable<PrintPreviewResponse>;

    if (template.Usage === 'FromDetails') {
      if (!template.Collection) {
        this.message = `Please specify the collection in Metadata.`;
        this.loading = false;
        return of();
      }

      if (this.showDefinitionIdSelector(template) && !template.DefinitionId) {
        this.message = 'Please specify the definition in Metadata.';
        this.loading = false;
        return of();
      }

      if (!this.id) {
        this.message = `Please specify the ${this.detailsPickerLabel(template)} above`;
        this.loading = false;
        return of();
      }

      const args: PrintEntityByIdArguments = {
        culture,
      };

      obs$ = this.printingTemplatesApi.previewById(this.id, template, args);
    } else if (template.Usage === 'FromMasterAndDetails') {
      if (!template.Collection) {
        this.message = `Please specify the collection`;
        this.loading = false;
        return of();
      }

      const args: PrintEntitiesArguments = {
        culture,
        filter: this.filter,
        orderby: this.orderby,
        top: this.top,
        skip: this.skip
      };

      obs$ = this.printingTemplatesApi.previewByFilter(template, args);
    } else {
      const args: PrintArguments = {
        culture
      };

      obs$ = this.printingTemplatesApi.preview(template, args);
    }

    this.loading = true;
    return obs$.pipe(
      tap((res: PrintPreviewResponse) => {
        this.fileDownloadName = res.DownloadName;

        const blob = new Blob([res.Body], { type: 'text/html' });
        this.blob = blob;

        if (!!this.url) {
          window.URL.revokeObjectURL(this.url);
        }
        this.url = window.URL.createObjectURL(blob) + '#toolbar=0&navpanes=0&scrollbar=0';

        // We just made it so it's definitely safe
        (this.iframe.nativeElement as HTMLIFrameElement).contentWindow.location.replace(this.url);
        this.fileSizeDisplay = fileSizeDisplay(blob.size);
        this.loading = false;
      }),
      catchError((friendlyError: FriendlyError) => {
        this.error = friendlyError.error || 'Unknown error.';
        this.loading = false;
        return of(null);
      })
    );
  }

  public get allCollections(): SelectorChoice[] {
    return collectionsWithEndpoint(this.workspace, this.translate);
  }

  public showDefinitionIdSelector(model: PrintingTemplateForSave): boolean {
    return !!model && !!model.Collection && !!metadata[model.Collection](this.workspace, this.translate, null).definitionIds;
  }

  public allDefinitionIds(model: PrintingTemplateForSave): SelectorChoice[] {
    if (!!model && !!model.Collection) {
      const func = metadata[model.Collection];
      const desc = func(this.workspace, this.translate, null);
      if (!!desc.definitionIds && !desc.definitionIdsArray) {
        desc.definitionIdsArray = desc.definitionIds
          .map(defId => ({ value: defId, name: func(this.workspace, this.translate, defId).titlePlural }));
      }

      return desc.definitionIdsArray;
    } else {
      return null;
    }
  }

  public showParametersSection(model: PrintingTemplateForSave): boolean {
    return !!model.Usage;
  }

  onPrint(_: PrintingTemplateForSave) {
    printBlob(this.blob);
  }

  public get disablePrint(): boolean {
    return !this.blob; // this.loading || !!this.error || !this.safeUrl;
  }

  public onDownload(_: PrintingTemplateForSave) {
    if (!!this.blob) {
      downloadBlob(this.blob, this.fileDownloadName);
    }
  }

  public get disableDownload(): boolean {
    return !this.blob; // this.loading || !!this.error || !this.safeUrl;
  }

  onRefresh(template: PrintingTemplateForSave) {
    if (!this.loading) {
      this.fetch(template);
    }
  }

  public get showRefresh(): boolean {
    return !this.loading;
  }

  public get showSpinner(): boolean {
    return this.loading;
  }

  public showLanguageToggle(model: PrintingTemplateForSave): boolean {
    return (this.showLang(1, model) ? 1 : 0) +
      (this.showLang(2, model) ? 1 : 0) +
      (this.showLang(3, model) ? 1 : 0) > 1;
  }

  public langDisplay(lang: 1 | 2 | 3): string {
    if (lang === 1) {
      return this.ws.settings.PrimaryLanguageName;
    }
    if (lang === 2) {
      return this.ws.settings.SecondaryLanguageName;
    }
    if (lang === 3) {
      return this.ws.settings.TernaryLanguageName;
    }

    return '';
  }

  public onLang(lang: 1 | 2 | 3, model: PrintingTemplateForSave): void {
    if (this.lang !== lang) {
      this.lang = lang;
      // this.details.urlStateChange();
      this.onDefinitionChange(model);
    }
  }

  public isLang(lang: 1 | 2 | 3): boolean {
    return this.lang === lang;
  }

  public showLang(lang: 1 | 2 | 3, model: PrintingTemplateForSave): boolean {
    return (lang === 1 && !!model.SupportsPrimaryLanguage) ||
      (lang === 2 && !!model.SupportsSecondaryLanguage && !!this.ws.settings.SecondaryLanguageId) ||
      (lang === 3 && !!model.SupportsTernaryLanguage && !!this.ws.settings.TernaryLanguageId);
  }

  public showMasterAndDetailsParams(model: PrintingTemplateForSave) {
    return model.Usage === 'FromMasterAndDetails';
  }

  public showDetailsParams(model: PrintingTemplateForSave) {
    return model.Usage === 'FromDetails';
  }

  public showCollectionAndDefinition(model: PrintingTemplateForSave) {
    return model.Usage === 'FromDetails' || model.Usage === 'FromMasterAndDetails';
  }

  private _currentModel: PrintingTemplateForSave;
  public watch(model: PrintingTemplateForSave): boolean {
    // If it's a different model than last time, reset the params and refetch
    const s = this.state.detailsState;
    if (s.modelCollection !== (model.Collection || null) ||
      s.modelDefinitionId !== (model.DefinitionId || null)) {
      s.modelCollection = model.Collection || null;
      s.modelDefinitionId = model.DefinitionId || null;

      // Current state might be inconsistent with the new collection and defId
      this.resetState();
      this.fetch(model);

      // If it's the same model, but we just returned to the screen, just fetch
    } else if (!this.loading && !this.error && !this.message && !this.blob) {
      this.fetch(model);

      // If it's the same model but refreshed from the backend, fetch again (in case in changed)
    } else if (this._currentModel !== model) {
      this.fetch(model);
    }

    this._currentModel = model;
    return true;
  }

  public detailsPickerLabel(model: PrintingTemplateForSave): string {
    if (!!model && !!model.Collection) {
      const descFunc = metadata[model.Collection];
      const desc = descFunc(this.workspace, this.translate, model.DefinitionId);
      return desc.titleSingular();
    }

    return ''; // Should not reach here in theory
  }

  public get filter(): string {
    return this.state.detailsState.filter;
  }

  public set filter(v: string) {
    this.state.detailsState.filter = v;
  }

  public get orderby(): string {
    return this.state.detailsState.orderby;
  }

  public set orderby(v: string) {
    this.state.detailsState.orderby = v;
  }

  public get top(): number {
    return this.state.detailsState.top;
  }

  public set top(v: number) {
    this.state.detailsState.top = v;
  }

  public get skip(): number {
    return this.state.detailsState.skip;
  }

  public set skip(v: number) {
    this.state.detailsState.skip = v;
  }

  public get id(): number | string {
    return this.state.detailsState.id;
  }

  public set id(v: number | string) {
    this.state.detailsState.id = v;
  }

  public get lang(): 1 | 2 | 3 {
    return this.state.detailsState.lang;
  }

  public set lang(v: 1 | 2 | 3) {
    this.state.detailsState.lang = v;
  }

  public onKeydown(elem: HTMLTextAreaElement, $event: KeyboardEvent, model: PrintingTemplate) {
    onCodeTextareaKeydown(elem, $event, v => model.Body = v);
  }
}

// tslint:disable:no-trailing-whitespace
const defaultBody = `<!DOCTYPE html>
<html lang="{{ $Lang }}">
<head>
    <meta charset="UTF-8">
    <title>{{ 'Document' }}</title>
    <style>

        /* Printing CSS: Remove if not for printing */
        @media screen {
            body {
                background-color: #F9F9F9;
            }
            .page {
                margin-left: auto;
                margin-right: auto;
                margin-top: 1rem;
                margin-bottom: 1rem;
                border: 1px solid lightgrey;
                background-color: white;
                box-shadow: rgba(60, 64, 67, 0.15) 0px 1px 3px 1px;
                box-sizing: border-box;
                width: 210mm;
                min-height: 297mm;
                padding: 0.5in;
            }
        }
        @page {
            margin: 0.5in;
            size: A4 Portrait;
        }
        .page {
            break-after: page;
        }
        /* End Printing CSS */
        
        * {
            font-family: sans-serif;
            box-sizing: border-box;
        }
        
        body {
            margin: 0;
        }
        
        body.rtl {
            direction: rtl;
        }
        
        /* More CSS Here */
    
    </style>
</head>
<body class="{{ IF($IsRtl, 'rtl', '') }}">
    <div class="page">
        <!-- HTML Template Here -->
        
    </div>
</body>
</html>`;


const defaultBodyOld = `<!DOCTYPE html>
<html lang="{{ $Lang }}">
<head>
    <meta charset="UTF-8">
    <title>{{ 'Document' }}</title>
    <style>

        /* Printing CSS: Remove if not for printing */
        {{ *define $PageSize as 'A4' }} /* https://mzl.la/3d8twxF */
        {{ *define $Orientation as 'Portrait' }} /* 'Portrait', 'Landscape' */
        {{ *define $Margins as '0.5in' }} /* The page margins */
        @media screen {
            body {
                background-color: #F9F9F9;
            }
            .page {
                margin-left: auto;
                margin-right: auto;
                margin-top: 1rem;
                margin-bottom: 1rem;
                border: 1px solid lightgrey;
                background-color: white;
                box-shadow: rgba(60, 64, 67, 0.15) 0px 1px 3px 1px;
                box-sizing: border-box;
                width: {{ PreviewWidth($PageSize, $Orientation) }};
                min-height: {{ PreviewHeight($PageSize, $Orientation) }};
                padding: {{ $Margins }};
            }
        }
        @page {
            margin: {{ $Margins }};
            size: {{ $PageSize }} {{ $Orientation }};
        }
        /* End Printing CSS */
        
        * {
            font-family: sans-serif;
        }
        
        body {
            margin: 0;
        }
        
        /* More CSS Here */
    
    </style>
</head>
<body class="{{ IF($IsRtl, 'rtl', '') }}">
    <div class="page">
        <!-- HTML Template Here -->
        
    </div>
</body>
</html>`;
