// tslint:disable:member-ordering
import { Component, EventEmitter, Input, OnDestroy, OnInit, Output } from '@angular/core';
import { merge, Observable, of, Subject, Subscription } from 'rxjs';
import { catchError, finalize, switchMap, tap } from 'rxjs/operators';
import { TemplateParameterForClient } from '~/app/data/dto/definitions-for-client';
import { MessageCommandPreview } from '~/app/data/dto/message-command-preview';
import { PropVisualDescriptor } from '~/app/data/entities/base/metadata';
import { descFromControlOptions, updateOn } from '~/app/data/util';
import { ReportArguments, WorkspaceService } from '~/app/data/workspace.service';

@Component({
  selector: 't-message-previewer',
  templateUrl: './message-previewer.component.html',
  styles: [
  ]
})
export class MessagePreviewerComponent implements OnInit, OnDestroy {

  private _subscriptions = new Subscription();
  private notifyFetch$ = new Subject<void>();

  @Input()
  messageCommandPreview: () => Observable<MessageCommandPreview>;

  @Input()
  refresh: Observable<void>;

  @Input()
  parameters: TemplateParameterForClient[] = [];

  @Output()
  argumentsChange = new EventEmitter<ReportArguments>();

  constructor(private workspace: WorkspaceService) { }

  ngOnInit(): void {
    // Hook the observables
    let allSignals: Observable<void> = this.notifyFetch$;
    if (!!this.refresh) {
      allSignals = merge(this.refresh, allSignals);
    }

    this._subscriptions.add(allSignals.pipe(
      switchMap(_ => this.doFetch())
    ).subscribe());

    // First time always fetch
    this.fetch();
  }

  ngOnDestroy(): void {
    if (!!this._subscriptions) {
      this._subscriptions.unsubscribe();
    }
  }

  public isMessageCommandLoading = false;
  public messageCommandError: () => string;

  public messageCommand: MessageCommandPreview;

  private fetch() {
    this.notifyFetch$.next();
  }

  private doFetch(): Observable<void> {

    this.messageCommandError = null;
    this.messageCommand = null;
    this.isMessageCommandLoading = true;

    return this.messageCommandPreview().pipe(
      tap((cmd: MessageCommandPreview) => {
        this.messageCommand = cmd;
      }),
      catchError(friendlyError => {
        this.messageCommandError = () => friendlyError.error;
        return of(null);
      }),
      finalize(() => {
        this.isMessageCommandLoading = false;
      })
    );
  }
}
