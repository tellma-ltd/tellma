import { Component, OnInit, OnDestroy, ViewChild, TemplateRef, HostListener, Input } from '@angular/core';
import { Subject, Observable, of } from 'rxjs';
import { WorkspaceService, DetailsStatus } from '~/app/data/workspace.service';
import { ApiService } from '~/app/data/api.service';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateService } from '@ngx-translate/core';
import { switchMap, catchError, tap } from 'rxjs/operators';
import { mergeEntitiesInWorkspace } from '~/app/data/util';
import { ICanDeactivate } from '~/app/data/unsaved-changes.guard';
import { handleFreshSettings } from '~/app/data/tenant-resolver.guard';
import { StorageService } from '~/app/data/storage.service';
import { GetEntityResponse } from '~/app/data/dto/get-entity-response';
import { SaveSettingsResponse } from '~/app/data/dto/save-settings-response';
import { applyServerErrors, clearServerErrors } from '../details/details.component';
import { SettingsBase } from '~/app/data/entities/base/settings-base';

@Component({
  selector: 't-settings',
  templateUrl: './settings.component.html',
  styles: []
})
export class SettingsComponent implements OnInit, OnDestroy, ICanDeactivate {


  private notifyFetch$: Subject<void>;
  private notifyDestruct$ = new Subject<void>();
  private crud = this.api.settingsFactory('', this.notifyDestruct$); // Just for intellisense
  private detailsStatus: DetailsStatus;

  private _viewModel: SettingsBase;
  private _viewModelJson: string;
  private _editModel: SettingsBase;

  private _errorMessage: string; // in the document area itself
  private _modalErrorMessage: string; // in the modal
  private _unboundServerErrors: string[] = [];

  @Input()
  title: string;

  @Input()
  endpoint: string;

  @Input()
  expand: string;

  @Input()
  view: string;

  @Input()
  template: TemplateRef<any>;

  @ViewChild('errorModal', { static: true })
  public errorModal: TemplateRef<any>;

  @ViewChild('unsavedChangesModal', { static: true })
  public unsavedChangesModal: TemplateRef<any>;

  @HostListener('window:beforeunload', ['$event'])
  doSomething($event: BeforeUnloadEvent) {
    if (this.isDirty) {
      $event.returnValue = this.translate.instant('UnsavedChangesConfirmationMessage');
    }
  }

  constructor(
    private workspace: WorkspaceService, private api: ApiService, private storage: StorageService,
    public modalService: NgbModal, private translate: TranslateService) {


    // When the notifyFetch$ subject fires, cancel existing backend
    // call and dispatch a new backend call
    this.notifyFetch$ = new Subject<any>();
    this.notifyFetch$.pipe(
      switchMap(() => this.doFetch())
    ).subscribe();
  }

  public displayModalError(errorMessage: string) {
    // shows the error message in a dismissable modal
    this._modalErrorMessage = errorMessage;
    this.modalService.open(this.errorModal);
  }

  ngOnInit() {
    // As if the screen is opened a new
    this.clearErrors();

    // initialize the API service
    this.crud = this.api.settingsFactory(this.endpoint, this.notifyDestruct$);

    // Fetch the data of the screen
    this.fetch();
  }

  ngOnDestroy() {
    // Cancel any backend operations
    this.notifyDestruct$.next();
  }

  public canDeactivate(): boolean | Observable<boolean> {
    if (this.isDirty) {

      // IF there are unsaved changes, prompt the user asking if they would like them discarded
      const modal = this.modalService.open(this.unsavedChangesModal);

      // capture the user's decision in a subject:
      // first action when the user presses one of the two buttons
      // second func is when the user dismisses the modal with x or ESC or clicking the background
      const decision$ = new Subject<boolean>();
      modal.result.then(
        v => { decision$.next(v); decision$.complete(); },
        _ => { decision$.next(false); decision$.complete(); }
      );

      // return the subject that will eventually emit the user's decision
      return decision$;

    } else {

      // IF there are no unsaved changes, the navigation can happily proceed
      return true;
    }
  }

  private clearErrors(): void {
    this._errorMessage = null;
    this._modalErrorMessage = null;
    this._unboundServerErrors = [];
  }

  private fetch() {
    this.notifyFetch$.next(null);
  }

  private doFetch(): Observable<void> {
    // clear the errors before refreshing
    this.clearErrors();

    // ELSE fetch the record from server
    // first show the rotator
    this.detailsStatus = DetailsStatus.loading;

    // server call
    return this.crud.get({ expand: this.expand }).pipe(
      tap((response: GetEntityResponse<SettingsBase>) => {

        // add the settings locally
        this._viewModel = response.Result;

        // Add related items to the workspace
        mergeEntitiesInWorkspace(response.RelatedEntities, this.workspace);

        // Notify everyone
        this.workspace.notifyStateChanged();

        // display the settings
        this.detailsStatus = DetailsStatus.loaded;

      }),
      catchError((friendlyError) => {
        this.detailsStatus = DetailsStatus.error;
        this._errorMessage = friendlyError.error;
        return of(null);
      })
    );
  }

  // UI Bindings

  get isDirty(): boolean {
    // TODO This may cause sluggishness for large DTOs, we'll look into ways of optimizing it later
    return this.isEdit && this._viewModelJson !== JSON.stringify(this._editModel);
  }

  get isEdit(): boolean {
    return this.detailsStatus === DetailsStatus.edit;
  }

  get placement() {
    return this.workspace.ws.isRtl ? 'bottom-right' : 'bottom-left';
  }

  get errorMessage() {
    return this._errorMessage;
  }

  get modalErrorMessage() {
    return this._modalErrorMessage;
  }

  get activeModel(): SettingsBase {
    return this.isEdit ? this._editModel : this._viewModel;
  }

  get showSpinner(): boolean {
    return this.detailsStatus === DetailsStatus.loading;
  }

  get showDocument(): boolean {
    return this.detailsStatus === DetailsStatus.loaded ||
      this.detailsStatus === DetailsStatus.edit;
  }

  get showRefresh(): boolean {
    return !this.isEdit;
  }

  get showViewToolbar(): boolean {
    return !this.showEditToolbar;
  }

  get showEditToolbar(): boolean {
    return this.detailsStatus === DetailsStatus.edit;
  }

  get showErrorMessage(): boolean {
    return this.detailsStatus === DetailsStatus.error;
  }

  onRefresh(): void {
    if (this.detailsStatus !== DetailsStatus.loading) {
      this.fetch();
    }
  }

  onEdit(): void {
    if (!this.canEdit) {
      return;
    }

    if (this._viewModel) {
      // clone the model (to allow for canceling changes)
      this._viewModelJson = JSON.stringify(this._viewModel);
      this._editModel = JSON.parse(this._viewModelJson);

      // show the edit view
      this.detailsStatus = DetailsStatus.edit;
    }
  }

  get canEditPermissions(): boolean {

    return this.workspace.currentTenant.canUpdate(this.view, null);
  }

  get canEdit(): boolean {
    return !!this.showDocument && this.canEditPermissions;
  }

  get editTooltip(): string {
    return this.canEditPermissions ? '' : this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions');
  }

  onSave(): void {
    // if it's new the user expects a save to happen even if there is no red asterisk
    if (!this.isDirty) {
      // since no changes, don't save to the database
      // just go back to view mode
      this.clearErrors();
      clearServerErrors(this._editModel);
      this._editModel = null;
      this.detailsStatus = DetailsStatus.loaded;
    } else {

      // clear any errors displayed
      this.clearErrors();
      clearServerErrors(this._editModel);

      // prepare the save observable
      this.crud.save(this._editModel, { expand: this.expand, returnEntities: true }).subscribe(
        (response: SaveSettingsResponse<SettingsBase>) => {

          // update the workspace with the DTO from the server
          this._viewModel = response.Result;
          mergeEntitiesInWorkspace(response.RelatedEntities, this.workspace);

          // Notify everyone
          this.workspace.notifyStateChanged();

          // Update the cache with fresh versions
          if (!!response.SettingsForClient) {
            handleFreshSettings(response.SettingsForClient,
              this.workspace.ws.tenantId, this.workspace.currentTenant, this.storage);
          }

          // in screen mode always close the edit view
          this.detailsStatus = DetailsStatus.loaded;

          // remove the local copy the user was editing
          this._editModel = null;

        },
        (friendlyError) => {
          if (this.workspace.current.unauthorized) {
            // The user will be redirected away from the tenant anyways
            // and the screen they're taken to will show an appropriate
            // error message, so no need to show a modal here
            return;
          }

          if (friendlyError.status === 422) {
            // This handles 422 ModelState errors
            this.apply422ErrorsToModel(friendlyError.error);
          } else {
            this.displayModalError(friendlyError.error);
          }
        }
      );
    }
  }

  public onCancel(): void {
    // prompt the user manually, since the Angular Router isn't involved
    const canCancel = this.canDeactivate();
    if (canCancel instanceof Observable) {
      canCancel.subscribe(can => {
        if (can) {
          this.doCancel();
        }
      });
    } else if (canCancel) {
      this.doCancel();
    }
  }

  private doCancel(): void {
    // clear the edit model and error messages
    this._editModel = null;
    this.clearErrors();

    // ... and then close the edit form
    this.detailsStatus = DetailsStatus.loaded;
  }

  /**
   * Handles 422 Unprocessible Entity errors from a save operation, it distributes the
   * errors on the entity that was saved and all its related weak entities, by parsing
   * the paths and adding the messages in the serverErrors dictionary of the target entity
   */
  private apply422ErrorsToModel(errors: { [path: string]: string[] }) {
    this._unboundServerErrors = [];
    const serverErrors = applyServerErrors(this._editModel, errors);
    const keys = Object.keys(serverErrors);
    keys.forEach(key => {
      serverErrors[key].forEach(error => {
        this._unboundServerErrors.push(error);
      });
    });
  }
}
