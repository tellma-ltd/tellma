import { Location } from '@angular/common';
import { Component, EventEmitter, Input, OnDestroy, OnInit, TemplateRef, ViewChild, Output } from '@angular/core';
import { ActivatedRoute, ParamMap, Router, Params } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateService } from '@ngx-translate/core';
import { catchError, switchMap, tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { DtoForSaveKeyBase } from '~/app/data/dto/dto-for-save-key-base';
import { GetByIdResponse } from '~/app/data/dto/get-by-id-response';
import { EntitiesResponse } from '~/app/data/dto/get-response';
import { addSingleToWorkspace, addToWorkspace } from '~/app/data/util';
import { DetailsStatus, MasterDetailsStore, WorkspaceService } from '~/app/data/workspace.service';
import { ICanDeactivate } from '~/app/data/unsaved-changes.guard';
import { DtoKeyBase } from '~/app/data/dto/dto-key-base';
import { Subject, Observable, of } from 'rxjs';

@Component({
  selector: 'b-details',
  templateUrl: './details.component.html',
  styleUrls: ['./details.component.scss']
})
export class DetailsComponent implements OnInit, OnDestroy, ICanDeactivate {

  @Input()
  viewId: string; // for the permissions

  @Input()
  collection: string;

  @Input()
  expand: string;

  @Input()
  masterCrumb: string;

  @Input()
  detailsCrumb: string;

  @Input()
  documentHeaderTemplate: TemplateRef<any>;

  @Input()
  documentTemplate: TemplateRef<any>;

  @Input()
  sidebarTemplate: TemplateRef<any>;

  @Input()
  savePreprocessing: (mode: DtoForSaveKeyBase) => void;

  @Input()
  workspaceApplyFns: { [collection: string]: (stale: DtoKeyBase, fresh: DtoKeyBase) => DtoKeyBase };

  @Input()
  actions: {
    template: TemplateRef<any>,
    action: (model: DtoForSaveKeyBase) => void,
    canAction?: (model: DtoForSaveKeyBase) => boolean,
    actionTooltip?: (model: DtoForSaveKeyBase) => string,
    showAction?: (model: DtoForSaveKeyBase) => boolean
  }[] = [];

  @Input() // popup: only the title and the document are visible
  mode: 'popup' | 'screen' = 'screen';

  // apiEndpoint and idString both represent the identity of an abstract "instance" of this screen
  // so when they change it will be as if a screen closed and another screen opened from the point
  // of view of the user, for performance reasons Angular does not destroy and recreate the component
  // if the same component is still used but its input has changed, so to simulate a screen change
  // we call destroy and init in the property setters of these two properties, if this is not the
  // first time they are being set, the same pattern is used in the master screen

  @Input()
  public set apiEndpoint(v: string) {
    // apiEndpoint cannot be reset to null
    if (!!v && this._apiEndpoint !== v) {
      if (this.alreadyInit) {
        this.ngOnDestroy();
      }

      this._apiEndpoint = v;

      if (this.alreadyInit) {
        this.ngOnInit();
      }
      // this.fetch();
    }
  }

  public get apiEndpoint() {
    return this._apiEndpoint;
  }

  @Input()
  public set idString(v: string) {
    // idString cannot be reset to null
    if (!!v && this._idString !== v) {
      if (this.alreadyInit) {
        this.ngOnDestroy();
      }

      this._idString = v;

      if (this.alreadyInit) {
        this.ngOnInit();
      }
      // this.fetch();
    }
  }

  public get idString() {
    return this._idString;
  }

  @Output()
  saved = new EventEmitter<number | string>();

  @Output()
  cancel = new EventEmitter<void>();

  @ViewChild('errorModal')
  public errorModal: TemplateRef<any>;

  @ViewChild('unsavedChangesModal')
  public unsavedChangesModal: TemplateRef<any>;

  private alreadyInit: boolean;
  private _idString: string;
  private _apiEndpoint: string;
  private _editModel: DtoForSaveKeyBase;
  private notifyFetch$: Subject<void>;
  private notifyDestruct$ = new Subject<void>();
  private localState = new MasterDetailsStore();  // Used in popup mode
  private _errorMessage: string; // in the document area itself
  private _modalErrorMessage: string; // in the modal
  private _unboundServerErrors: string[]; // in the modal
  private _viewModelJson: string;
  private crud = this.api.crudFactory(this.apiEndpoint, this.notifyDestruct$); // Just for intellisense


  // Moved below the fields to keep tslint happy
  @Input()
  createFunc: () => DtoForSaveKeyBase = () => ({ Id: null, EntityState: 'Inserted' })

  @Input()
  isInactive: (model: DtoForSaveKeyBase) => string = (model: DtoForSaveKeyBase) =>
    (model['IsActive'] == null || model['IsActive'] === false) ? 'Error_CannotModifyInactiveItemPleaseActivate' : null

  @Input()
  cloneFunc: (item: DtoForSaveKeyBase) => DtoForSaveKeyBase = (item: DtoForSaveKeyBase) => {
    if (!!item) {
      const clone = <DtoForSaveKeyBase>JSON.parse(JSON.stringify(item));
      clone.Id = null;
      clone.EntityState = 'Inserted';

      return clone;
    } else {
      console.error('Cloning a non existing item');
      return null;
    }
  }

  constructor(private workspace: WorkspaceService, private api: ApiService, private location: Location,
    private router: Router, private route: ActivatedRoute, public modalService: NgbModal, private translate: TranslateService) {
    // the constructor contains initializations and wiring
    // that survives over the lifetime of the component itself
    // even if apiEndpoint or idString change

    this.route.paramMap.subscribe((params: ParamMap) => {
      // the id parameter from the URI is only avaialble in screen mode
      // when it changes set idString which triggers a new refresh
      if (params.has('id')) {
        this.idString = params.get('id');
      }
    });

    // when the notifyFetch$ subject fires, cancel existing backend
    // call and dispatch a new backend call
    this.notifyFetch$ = new Subject<any>();
    this.notifyFetch$.pipe(
      switchMap(() => this.doFetch())
    ).subscribe();
  }

  ngOnInit() {
    // as if the screen is opened a new
    this.localState = new MasterDetailsStore();
    this._errorMessage = null;
    this._modalErrorMessage = null;
    this._unboundServerErrors = [];
    this.crud = this.api.crudFactory(this.apiEndpoint, this.notifyDestruct$);
    this._viewModelJson = null;

    // Fetch the data of the screen based on apiEndpoint and idString
    this.fetch();

    // This signals the setters of apiEndpoint and idString to manually
    // invoke ngOnInit next time they are called
    this.alreadyInit = true;
  }

  ngOnDestroy() {
    // cancel any backend operations
    this.notifyDestruct$.next();
  }

  private fetch() {
    this.notifyFetch$.next(null);
  }

  private get cloneId(): string {
    return this.route.snapshot.paramMap.get('cloneId');
  }

  private doFetch(): Observable<void> {
    // clear the errors before refreshing
    this.clearErrors();

    // grab the configured state
    const s = this.state;

    // calculate some logical values
    const cloneId = this.cloneId;
    const isCloning = !!cloneId;
    const isNewNotClone = this.isNew && !isCloning;
    const isCloneOfAvailableItem = isCloning && !!this.workspace.current[this.collection][cloneId];

    // the block in the first IF statement returns immediately, it's either
    // a create new or a clone of an item that exists in the workspace, if
    // neither is true, then we have to fetch the record from the server,
    // then either display it or clone it
    if (isNewNotClone || isCloneOfAvailableItem) {

      if (isCloneOfAvailableItem) {
        // IF it's a cloning operation, clone the item from workspace
        const item = this.workspace.current[this.collection][cloneId];
        this._editModel = this.cloneFunc(item);
      } else {
        // IF it's create new, don't fetch anything, create an item in memory
        this._editModel = this.createFunc();
      }

      // marks it as non-dirty until the user makes the first change
      this._viewModelJson = JSON.stringify(this._editModel);

      // Show edit form
      s.detailsStatus = DetailsStatus.edit;

      // return
      // IF it's a cloning operation, clone the item from workspace
      return of();

    } else {
      // IF it's the last viewed item also don't do anything
      if (!!s.detailsId && s.detailsId.toString() === this.idString && s.detailsStatus === DetailsStatus.loaded) {
        // the application caches the last record that was viewed by the user
        // if the new id is equal to the Id of the last record then just display
        // that last record. This is helpful when navigating to Id after a create new
        return of();
      } else {
        // ELSE fetch the record from server
        // first show the rotator
        s.detailsStatus = DetailsStatus.loading;

        // if we're cloning we need to fetch the clone id
        // otherwise we need to fetch the usual id input of the screen
        const id = isCloning ? cloneId : this.idString;

        // server call
        return this.crud.getById(id, { expand: this.expand }).pipe(
          tap((response: GetByIdResponse) => {

            // add the server item to the workspace
            this.state.detailsId = addSingleToWorkspace(response, this.workspace, this.workspaceApplyFns);

            if (isCloning) {
              // call the same method again but this time the cloned
              // item is immediately available in the workspace
              this.doFetch();

            } else {
              // display the item in readonly if it's a screen
              // or in edit if it's a popup
              if (this.mode === 'screen') {
                this.state.detailsStatus = DetailsStatus.loaded;

              } else {
                this.onEdit();
              }
            }
          }),
          catchError((friendlyError) => {
            this._errorMessage = friendlyError.error;
            this.state.detailsStatus = DetailsStatus.error;
            return of(null);
          })
        );
      }
    }
  }

  private clearErrors(): void {
    this._errorMessage = null;
    this._modalErrorMessage = null;
    this._unboundServerErrors = [];
  }

  public get state(): MasterDetailsStore {
    // important to always reference the source, and not keep a local reference
    // on some occasions the source can be reset and using a local reference can cause bugs
    if (this.mode === 'popup') {

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
    if (!this.workspace.current.mdState[this.apiEndpoint]) {
      // if (this.mode === 'screen') {
      //   this.workspace.current.mdState = {}; // This forces any other master/details screen to refresh
      // }

      this.workspace.current.mdState[this.apiEndpoint] = new MasterDetailsStore();
    }

    return this.workspace.current.mdState[this.apiEndpoint];
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

  public displayModalError(errorMessage: string) {
    // shows the error message in a dismissable modal
    this._modalErrorMessage = errorMessage;
    this.modalService.open(this.errorModal);
  }

  get viewModel(): DtoForSaveKeyBase {
    // view data is always directly referencing the global workspace
    // this way, un update to a record in the global workspace automatically
    // updates all places where this record is displayed... nifty
    const s = this.state;
    return !!s.detailsId ? this.workspace.current[this.collection][s.detailsId] : null;
  }

  public handleActionError = (friendlyError: any) => {

    // This handles 422 ModelState errors
    if (friendlyError.status === 422) {
      this._unboundServerErrors = [];
      const serverErrors = applyServerErrors([this.activeModel], friendlyError.error);
      const keys = Object.keys(serverErrors);
      keys.forEach(key => {
        serverErrors[key].forEach(error => {
          this._unboundServerErrors.push(error);
        });
      });

    } else {
      this.displayModalError(friendlyError.error);
    }
  }

  ////// UI Bindings

  get errorMessage() {
    return this._errorMessage;
  }

  get modalErrorMessage() {
    return this._modalErrorMessage;
  }

  get unboundServerErrors(): string[] {
    return this._unboundServerErrors;
  }

  get activeModel(): DtoForSaveKeyBase {
    return this.isEdit ? this._editModel : this.viewModel;
  }

  get showSpinner(): boolean {
    return this.state.detailsStatus === DetailsStatus.loading;
  }

  get showDocument(): boolean {
    return this.state.detailsStatus === DetailsStatus.loaded ||
      this.state.detailsStatus === DetailsStatus.edit;
  }

  get showSidebar(): boolean {
    return !!this.sidebarTemplate && this.showDocument;
  }

  get showRefresh(): boolean {
    return !this.isEdit;
  }

  get isNew() {
    return this.idString === 'new';
  }

  get isDirty(): boolean {
    // TODO This may cause sluggishness for large DTOs, we'll look into ways of optimizing it later
    return this.isEdit && this._viewModelJson !== JSON.stringify(this._editModel);
  }

  get isEdit(): boolean {
    return this.state.detailsStatus === DetailsStatus.edit;
  }

  get isScreenMode() {
    return this.mode === 'screen';
  }

  get isPopupMode() {
    return this.mode === 'popup';
  }

  get showViewToolbar(): boolean {
    return !this.showEditToolbar;
  }

  get showEditToolbar(): boolean {
    return this.state.detailsStatus === DetailsStatus.edit;
  }

  get showErrorMessage(): boolean {
    return this.state.detailsStatus === DetailsStatus.error;
  }

  get showDelete(): boolean {
    return true; // TODO !!this.data[this.controller].delete;
  }

  get showClone(): boolean {
    return true; // TODO !!this.data[this.controller].delete;
  }

  onRefresh(): void {
    const s = this.state;
    if (s.detailsStatus !== DetailsStatus.loading) {

      // clear the cached item and fetch again
      s.detailsId = null;
      this.fetch();
    }
  }

  onCreate(): void {
    if (!this.canCreate) {
      return;
    }

    this.router.navigate(['..', 'new'], { relativeTo: this.route });
  }

  get canCreatePermissions(): boolean {
    return this.workspace.current.canCreate(this.viewId);
  }

  get canCreate(): boolean {
    return this.canCreatePermissions;
  }

  get createTooltip(): string {
    return this.canCreatePermissions ? '' : this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions');
  }

  onClone(): void {
    if (!this.canClone) {
      return;
    }

    const params: Params = {
      cloneId: this.activeModel.Id.toString()
    };

    this.router.navigate(['..', 'new', params], { relativeTo: this.route });
  }

  get canClone(): boolean {
    return !!this.activeModel && !!this.activeModel && this.canCreate;
  }

  get cloneTooltip(): string {
    return this.createTooltip;
  }

  onEdit(): void {
    if (!this.canEdit) {
      return;
    }

    if (this.viewModel) {

      // clone the model (to allow for canceling changes)
      this._viewModelJson = JSON.stringify(this.viewModel);
      this._editModel = JSON.parse(this._viewModelJson);

      // show the edit view
      this.state.detailsStatus = DetailsStatus.edit;
    }
  }

  get canEditPermissions(): boolean {

    const createdById = this.activeModel ? this.activeModel['CreatedById'] : null;
    return this.workspace.current.canUpdate(this.viewId, createdById);
  }

  get canEdit(): boolean {
    return this.activeModel && !this.isInactive(this.viewModel) && this.canEditPermissions;
  }

  get editTooltip(): string {
    const error = this.isInactive(this.viewModel);
    return this.canEditPermissions ? (!error ? '' : this.translate.instant(error))
      : this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions');
  }

  onSave(): void {
    // if it's new the user expects a save to happen even if there is no red asterisk
    if (!this.isDirty && !this.isNew) {
      if (this.mode === 'popup') {
        // In popup mode, just notify the outside world that a save has happened
        this.saved.emit(this._editModel.Id);

      } else {
        // since no changes, don't save to the database
        // just go back to view mode
        this.clearErrors();
        clearServerErrors(this._editModel);
        this._editModel = null;
        this.state.detailsStatus = DetailsStatus.loaded;
      }
    } else {

      // clear any errors displayed
      this.clearErrors();
      clearServerErrors(this._editModel);

      // we need the original value for when the save API call returns
      const isNew = this.isNew;

      // TODO: some screens may wish to customize this behavior for e.g. line item DTOs
      this._editModel.EntityState = isNew ? 'Inserted' : 'Updated';

      if (this.savePreprocessing) {
        this.savePreprocessing(this._editModel);
      }

      // prepare the save observable
      this.crud.save([this._editModel], { expand: this.expand, returnEntities: true }).subscribe(
        (response: EntitiesResponse) => {

          // update the workspace with the DTO from the server
          const s = this.state;
          s.detailsId = addToWorkspace(response, this.workspace, this.workspaceApplyFns)[0];

          // IF it's a new entity add it to the global state, (not the local one one even if inside a popup)
          if (isNew) {
            this.globalState.insert([s.detailsId]);
          }

          if (this.mode === 'popup') {
            // in popup mode, just notify the outside world that a save has happened
            this.onEdit(); // to replace the edit mode with the one from the server
            this.saved.emit(s.detailsId);

          } else {
            // in screen mode always close the edit view
            s.detailsStatus = DetailsStatus.loaded;

            // remove the local copy the user was editing
            this._editModel = null;

            // IF new and in screen mode, navigate to the Id just returned
            if (this.isNew) {
              this.router.navigate(['..', s.detailsId], { relativeTo: this.route });
            }
          }
        },
        (friendlyError) => this.handleActionError(friendlyError)
      );
    }
  }

  onCancel(): void {
    if (this.mode === 'popup') {
      // in popup mode, just notify the outside world that a cancel has happened
      this.cancel.emit();
    } else {
      // in screen mode...
      // remove the edit model
      if (this.isNew) {

        // this step in order to avoid the unsaved changes modal
        this.state.detailsStatus = DetailsStatus.loaded;

        // navigate back to the last screen
        this.location.back();

      } else {
        // clear the edit model and error messages
        this._editModel = null;
        this.clearErrors();

        // ... and then close the edit form
        this.state.detailsStatus = DetailsStatus.loaded;
      }
    }
  }

  onDelete(): void {
    // Assuming the entity is not new
    const id = this.viewModel.Id;
    this.crud.delete([id]).subscribe(
      () => {
        // remove from master and total of the global state
        this.globalState.delete([id]);

        // after a successful delete navigate back to the master
        this.router.navigate(['..'], { relativeTo: this.route });
      },
      (friendlyError) => this.handleActionError(friendlyError)
    );
  }

  get canDeletePermissions(): boolean {
    return this.canEditPermissions;
  }

  get canDelete(): boolean {
    return !!this.viewModel && this.canDeletePermissions;
  }

  get deleteTooltip(): string {
    return this.canDeletePermissions ? '' : this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions');
  }

  onNext(): void {
    this.router.navigate(['..', this.getNextId()], { relativeTo: this.route });
  }

  get canNext(): boolean {

    return !!this.getNextId();
  }

  onPrevious(): void {
    this.router.navigate(['..', this.getPreviousId()], { relativeTo: this.route });
  }

  get canPrevious(): boolean {
    return !!this.getPreviousId();
  }

  private getNextId(): number | string {
    const s = this.state;
    const id = this.idString;

    if (!!id) {
      const index = s.masterIds.findIndex(e => e.toString() === id);
      if (index !== -1 && index !== s.masterIds.length - 1) {
        const nextIndex = index + 1;
        const nextId = s.masterIds[nextIndex];
        return nextId;
      }
    }

    return null;
  }

  private getPreviousId(): number | string {
    const s = this.state;
    const id = this.idString;

    if (!!id) {
      const index = s.masterIds.findIndex(e => e.toString() === id);
      if (index > 0) {
        const prevIndex = index - 1;
        const prevId = s.masterIds[prevIndex];
        return prevId;
      }
    }

    return null;
  }

  get total(): number {
    return this.state.total;
  }

  get order(): number {
    const s = this.state;
    const id = this.idString;

    if (!!id) {
      const index = s.masterIds.findIndex(e => e.toString() === id);
      if (index !== -1) {
        return s.skip + index + 1;
      }
    }

    return null;
  }

  public get showNextAndPrevious(): boolean {
    return !!this.order;
  }

  onDocumentDblClick() {
    if (!this.isEdit && this.canEdit) {
      this.onEdit();
    }
  }

  public get flip() {
    // this is to flip the UI icons in RTL
    return this.workspace.ws.isRtl ? 'horizontal' : null;
  }

  public get actionsDropdownPlacement() {
    return this.workspace.ws.isRtl ? 'bottom-right' : 'bottom-left';
  }

  public canAction(action: {
    canAction?: (model: DtoForSaveKeyBase) => boolean,
  }): boolean {

    if (!!action.canAction) {
      return action.canAction(this.activeModel);
    } else {
      // true by default
      return true;
    }
  }

  public showAction(action: {
    showAction?: (model: DtoForSaveKeyBase) => boolean
  }): boolean {

    if (!!action.showAction) {
      return action.showAction(this.activeModel);
    } else {
      // true by default
      return true;
    }
  }

  public actionTooltip(action: {
    actionTooltip?: (model: DtoForSaveKeyBase) => string
  }): string {

    if (!!action.actionTooltip) {
      return action.actionTooltip(this.activeModel);
    } else {
      // don't show a tooltip by default
      return '';
    }
  }
}

export function applyServerErrors(entity: DtoForSaveKeyBase | DtoForSaveKeyBase[],
  errors: { [key: string]: string[] }): { [key: string]: string[] } {

  if (!entity) {
    return;
  }

  if (!errors) {
    return;
  }

  const paths = Object.keys(errors);
  const leftovers: { [key: string]: string[] } = {};
  for (let p = 0; p < paths.length; p++) {
    const path = paths[p];
    const steps = path.split('.');

    let current = entity;
    for (let s = 0; s < steps.length - 1; s++) {
      const step = steps[s];
      if (!current) {
        // Do nothing

      } else if (step.endsWith(']')) {
        // handle array
        const parts = step.substring(0, step.length - 1).split('[');
        const arrayPart = parts[0];
        const indexPart = +parts[1];

        if (isNaN(indexPart)) {
          // ignore a malformed error path
          leftovers[path] = errors[path];
          break;
        }

        if (!arrayPart) {
          // sometimes the entire entity is an array
          current = current[indexPart];
        } else {
          current = current[arrayPart][indexPart];
        }

      } else {
        // handle non-array
        current = current[step];
      }
    }

    if (!!current) {
      // we have to use the property indexer here otherwise typescript will complain
      // in reality we are setting serverErrors even if the target is an array, javascript
      // allows that but typescript doesn't
      if (!current['serverErrors']) {
        current['serverErrors'] = {};
      }

      const lastStep = steps[steps.length - 1];
      current['serverErrors'][lastStep] = errors[path];

    } else {
      leftovers[path] = errors[path];
    }
  }

  return leftovers;
}

export function clearServerErrors(entity: DtoForSaveKeyBase | DtoForSaveKeyBase[]): void {
  if (!entity) {
    // nothing to clear
    return;

  }
  // if errors exist remove them, they can exist even on arrays
  if (!!entity['serverErrors']) {
    delete entity['serverErrors'];
  }

  // if the property is an array, recursively clear the errors from all the items
  if (Array.isArray(entity)) {
    // loop over the array items and clear them
    for (let i = 0; i < entity.length; i++) {
      const item = entity[i];
      clearServerErrors(item);
    }
  } else if (!!entity.EntityState || !!entity.Id) {
    // if the property is a DTO loop over the navigation properties and recursively clear their errors
    const props = Object.keys(entity);
    for (let i = 0; i < props.length; i++) {
      const prop = props[i];
      const item = entity[prop];
      clearServerErrors(item);
    }
  }

}
