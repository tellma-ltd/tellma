// tslint:disable:member-ordering
import { Component, OnDestroy, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateService } from '@ngx-translate/core';
import { Observable, Subject, Subscription } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { MessageTemplateForClient, TemplateParameterForClient } from '~/app/data/dto/definitions-for-client';
import { IdResult } from '~/app/data/dto/id-result';
import { MessageCommandPreview } from '~/app/data/dto/message-command-preview';
import { PropVisualDescriptor } from '~/app/data/entities/base/metadata';
import { descFromControlOptions, FriendlyError, isSpecified, updateOn } from '~/app/data/util';
import { ReportArguments, WorkspaceService } from '~/app/data/workspace.service';

@Component({
  selector: 't-message-standalone',
  templateUrl: './message-standalone.component.html',
  styles: [
  ]
})
export class MessageStandaloneComponent implements OnInit, OnDestroy {

  private state = new MessageStore();
  private notifyDestruct$ = new Subject<void>();
  private _subscriptions = new Subscription();
  private messageTemplatesApi = this.api.messageTemplatesApi(null); // for intellisense

  @ViewChild('errorModal', { static: true })
  errorModal: TemplateRef<any>;

  @ViewChild('successModal', { static: true })
  successModal: TemplateRef<any>;

  constructor(
    private api: ApiService,
    private router: Router,
    private route: ActivatedRoute,
    private workspace: WorkspaceService,
    private translate: TranslateService,
    private modalService: NgbModal) { }

  ngOnInit() {
    this.messageTemplatesApi = this.api.messageTemplatesApi(this.notifyDestruct$); // for intellisense

    const onUrlChange = (params: ParamMap): boolean => {
      // This triggers changes on the screen
      let needsRefresh = false;

      const templateId = +params.get('templateId');
      const s = this.state;

      // Get the templateId
      const template = this.ws.definitions.MessageTemplates[templateId || 0];
      if (!template) {
        // TODO: Make it more graceful
        this.router.navigate(['page-not-found'], { relativeTo: this.route.parent, replaceUrl: true });
      }

      if (s.templateId !== templateId || s.template !== template) {
        s.templateId = templateId;
        s.template = template;
        needsRefresh = true; // Different template or different template Id
      }

      // Read the arguments from the URL
      for (const p of this.template.Parameters) {
        let urlValue: any;
        let keyLower: string;
        if (!!p.Key && params.has(keyLower = p.Key.toLowerCase())) {
          const urlStringValue = params.get(keyLower);
          urlValue = urlStringValue;
        }

        if ((s.arguments[p.Key] + '') !== urlValue) {
          s.arguments[p.Key] = urlValue;
          needsRefresh = true; // Different argument
        }
      }

      return needsRefresh;
    };

    if (onUrlChange(this.route.snapshot.paramMap)) {
      this.fetch();
    }

    // Listen to URL changes
    this._subscriptions.add(this.route.paramMap.subscribe(p => {
      if (onUrlChange(p)) {
        this.fetch();
      }
    }));

    // Listen to definition changes
    this._subscriptions.add(this.workspace.stateChanged$.subscribe(() => {
      const s = this.state;
      const templateId = s.templateId || 0;
      const newTemplate = this.ws.definitions.MessageTemplates[templateId];
      if (s.template !== newTemplate) {
        s.template = newTemplate;
        this.fetch();
      }
    }));
  }

  ngOnDestroy(): void {
    this._subscriptions.unsubscribe();
    this.notifyDestruct$.next();
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  public notifyFetch$ = new Subject<void>();

  private fetch(): void {
    this.notifyFetch$.next();
  }

  public get template(): MessageTemplateForClient { return this.state.template; }
  public preview: () => Observable<MessageCommandPreview> = () => {
    const s = this.state;
    delete s.command;
    return this.messageTemplatesApi.messageCommandPreviewByTemplateId(s.templateId, {}, s.arguments)
      .pipe(tap(cmd => s.command = cmd));
  }

  public onArgumentsChange(args: ReportArguments) {
    this.state.arguments = args;
    this.fetch();
  }

  // Parameters
  public get arguments(): ReportArguments { return this.state.arguments; }

  public updateOn(p: TemplateParameterForClient): 'change' | 'blur' {
    const desc = this.paramterDescriptor(p);
    return updateOn(desc);
  }

  public paramterDescriptor(p: TemplateParameterForClient): PropVisualDescriptor {
    return p.desc || (p.desc = descFromControlOptions(this.ws, p.Control, p.ControlOptions));
  }

  public label(p: TemplateParameterForClient): string {
    return this.ws.localize(p.Label, p.Label2, p.Label3);
  }

  public onArgumentChange() {
    this.fetch();
  }

  public get total(): number {
    return !!this.state.command ? this.state.command.Messages.length : 0;
  }

  // Toolbar

  public onConfirmSend() {
    const s = this.state;
    const template = s.template;
    const version = s.command.Version;

    const base$ = this.messageTemplatesApi.sendByMessage(template.MessageTemplateId, { }, version, this.arguments);

    base$.subscribe(
      (idResult: IdResult) => {
        this.commandId = idResult.Id;
        this.modalService.open(this.successModal);
      },
      (friendlyError: FriendlyError) => {
        this.modalErrorMessage = friendlyError.error;
        this.modalService.open(this.errorModal);
      });
  }

  public commandId: number;
  public modalErrorMessage: string;

  public get disableConfirmSend(): boolean {
    return this.areRequiredParamsMissing() || !this.state.command || !this.hasPermissionToSendMessage();
  }

  public sendMessageTooltip() {
    return this.hasPermissionToSendMessage() ? undefined : this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions');
  }

  public hasPermissionToSendMessage() {
    return !!this.template || this.workspace.currentTenant.canDo(`message-commands/${this.template.MessageTemplateId}`, 'Send', null);
  }

  public get dropdownPlacement() {
    return this.workspace.ws.isRtl ? 'bottom-right' : 'bottom-left';
  }

  get tenantId(): number {
    return this.workspace.ws.tenantId;
  }

  public areRequiredParamsMissing = () => {
    return !!this.template.Parameters && this.template.Parameters
      .some(p => p.IsRequired && !isSpecified(this.arguments[p.Key]));
  }
}

export class MessageStore {
  template: MessageTemplateForClient;
  templateId: number;
  arguments: ReportArguments = {};
  command: MessageCommandPreview;
}
