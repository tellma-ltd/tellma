import { Component, OnDestroy, OnInit, Input } from '@angular/core';
import { Subject, Observable, of, BehaviorSubject } from 'rxjs';
import { ActivatedRoute, ParamMap, Router, Params } from '@angular/router';
import { tap, catchError, switchMap } from 'rxjs/operators';
import { ApiService } from '../../data/api.service';
import { MeasurementUnit, MeasurementUnitForSave, MeasurementUnit_UnitType } from '../../data/dto/measurement-unit';
import { addToWorkspace, addSingleToWorkspace } from '../../data/util';
import { WorkspaceService, DetailsStatus, MasterDetailsStore } from '../../data/workspace.service';
import { DtoForSaveKeyBase } from '../../data/dto/dto-for-save-key-base';
import { EntitiesResponse } from '../../data/dto/get-response';
import { GetByIdResponse } from '../../data/dto/get-by-id-response';
import { TranslateService } from '@ngx-translate/core';
import { ListPicker } from 'tns-core-modules/ui/list-picker';

@Component({
  selector: 'b-measurement-units-details',
  templateUrl: './measurement-units-details.component.html',
  styleUrls: ['./measurement-units-details.component.scss']
})
export class MeasurementUnitsDetailsComponent implements OnDestroy, OnInit {

  @Input()
  apiEndpoint = 'measurement-units';

  @Input()
  collection = 'MeasurementUnits';

  @Input()
  expand: string;

  @Input()
  public set idString(v: string) {
    if (this._idString !== v) {
      this._idString = v;
      this.fetch();
    }
  }

  public get idString() {
    return this._idString;
  }

  public get unitTypeIndex(): number {
    return this.unitTypeChoices.indexOf(this.activeModel['UnitType']);
  }

  private _idString: string;
  private _editModel: DtoForSaveKeyBase;
  private notifyFetch$ = new BehaviorSubject<any>(null);
  private notifyDestruct$ = new Subject<void>();
  // private localState = new MasterDetailsStore();  // Used in popup mode
  private _errorMessage: string; // in the document area itself
  private _modalErrorMessage: string; // in the modal
  private _validationErrors: { [id: string]: string[] } = {}; // on the fields
  private crud = this.api.crudFactory(this.apiEndpoint, this.notifyDestruct$); // Just for intellisense
  private _viewModelJson;

  private _unitTypeChoices: string[];
  // private measurementUnitsApi = this.api.measurementUnitsApi(this.notifyDestruct$); // for intellisense

  selectedIndexChanged(args: any) {
    const picker = <ListPicker>args.object;
    const index = picker.selectedIndex;
    this.activeModel['UnitType'] = this.unitTypeChoices[index];
  }

  // Moved below the fields to keep tslint happy
  @Input()
  createFunc: () => DtoForSaveKeyBase = () => ({ Id: null, EntityState: 'Inserted' })

  @Input()
  cloneFunc: (id: string) => DtoForSaveKeyBase = (id: string) => {
    const item = this.workspace.current[this.collection][id];
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

  create = () => {
    const result = new MeasurementUnitForSave();
    result.UnitAmount = 1;
    result.BaseAmount = 1;
    return result;
  }

  get unitTypeChoices(): string[] {

    if (!this._unitTypeChoices) {
      this._unitTypeChoices = Object.keys(MeasurementUnit_UnitType)
        .filter(e => e !== 'Money');
    }

    return this._unitTypeChoices;
  }

  public unitTypeLookup(value: string): string {
    if (!value) {
      return '';
    }

    return MeasurementUnit_UnitType[value];
  }

  // public onActivate = (model: MeasurementUnit): void => {
  //   if (!!model && !!model.Id) {
  //     this.measurementUnitsApi.activate([model.Id], { ReturnEntities: true }).pipe(
  //       tap(res => addToWorkspace(res, this.workspace))
  //     ).subscribe(null, this.details.handleActionError);
  //   }
  // }

  // public onDeactivate = (model: MeasurementUnit): void => {
  //   if (!!model && !!model.Id) {
  //     this.measurementUnitsApi.deactivate([model.Id], { ReturnEntities: true }).pipe(
  //       tap(res => addToWorkspace(res, this.workspace))
  //     ).subscribe(null, this.details.handleActionError);
  //   }
  // }

  public showActivate = (model: MeasurementUnit) => !!model && !model.IsActive;
  public showDeactivate = (model: MeasurementUnit) => !!model && model.IsActive;


  ngOnDestroy() {
    this.notifyDestruct$.next();
  }



  /// Base Class



  constructor(private workspace: WorkspaceService, private api: ApiService,
    private router: Router, private route: ActivatedRoute, private translate: TranslateService) {
      console.log('constructor!!!');
    }

  ngOnInit() {
    console.log('ngOnInit!!!');

    this.crud = this.api.crudFactory(this.apiEndpoint, this.notifyDestruct$);

    // When the URI 'id' parameter changes
    // set idString which in turn fetches a new record
    this.route.paramMap.subscribe((params: ParamMap) => {
      // This triggers a refresh
      this.idString = params.get('id');
    });

    // When the notifyFetch$ subject fires, cancel existing backend
    // call and dispatch a new backend call
    this.notifyFetch$.pipe(
      switchMap(() => this.doFetch())
    ).subscribe();
  }

  private fetch() {
    this.notifyFetch$.next(null);
  }

  private get cloneId(): string {
    return this.route.snapshot.paramMap.get('cloneId');
  }

  private doFetch(): Observable<void> {
    console.log('DoFetch!!!');

    // clear the errors before refreshing
    this.clearErrors();

    // grab the configured state
    const s = this.state;
    if (this.isNew) {

      // IF it's create new, don't fetch anything, create an item in memory
      const cloneId = this.cloneId;
      if (!!cloneId) {
        this._editModel = this.cloneFunc(cloneId);
      } else {
        this._editModel = this.createFunc();
      }

      // marks it as non-dirty until the user makes the first change
      this._viewModelJson = JSON.stringify(this._editModel);

      // Show edit form
      s.detailsStatus = DetailsStatus.edit;

      // return
      return of();

    } else {
      console.log('DoFetch 2!!!');
      // IF it's the last viewed item also don't do anything
      if (!!s.detailsId && s.detailsId.toString() === this.idString && s.detailsStatus === DetailsStatus.loaded) {
        // the application caches the last record that was viewed by the user
        // if the new id is equal to the Id of the last record then just display
        // that last record. This is helpful when navigating to Id after a create new
        return of();
      } else {
        console.log('DoFetch 3!!!');

        // ELSE fetch the record from server
        // first show the rotator
        s.detailsStatus = DetailsStatus.loading;
        return this.crud.getById(this.idString, { expand: this.expand }).pipe(
          tap((response: GetByIdResponse) => {
            console.log('DoFetch 4 SUCCESS!!!');
            this.state.detailsId = addSingleToWorkspace(response, this.workspace);
            this.state.detailsStatus = DetailsStatus.loaded;
          }),
          catchError((friendlyError) => {
            console.log('DoFetch 4 ERROR!!!');
            this.state.detailsStatus = DetailsStatus.error;
            this._errorMessage = friendlyError.error;
            return of(null);
          })
        );
      }
    }
  }

  private clearErrors(): void {
    this._errorMessage = null;
    this._modalErrorMessage = null;
    this._validationErrors = {};
  }

  public get state(): MasterDetailsStore {
    // important to always reference the source, and not keep a local reference
    // on some occasions the source can be reset and using a local reference can cause bugs
    // screen mode on the other hand use the global state
    return this.globalState;
  }

  private get globalState(): MasterDetailsStore {
    if (!this.workspace.current.mdState[this.apiEndpoint]) {
      this.workspace.current.mdState[this.apiEndpoint] = new MasterDetailsStore();
    }

    return this.workspace.current.mdState[this.apiEndpoint];
  }

  // public canDeactivate(): boolean | Observable<boolean> {
  //   if (this.isDirty) {

  //     // IF there are unsaved changes, prompt the user asking if they would like them discarded
  //     const modal = this.modalService.open(this.unsavedChangesModal);

  //     // capture the user's decision in a subject:
  //     // first action when the user presses one of the two buttons
  //     // second func is when the user dismisses the modal with x or ESC or clicking the background
  //     const decision$ = new Subject<boolean>();
  //     modal.result.then(
  //       v => { decision$.next(v); decision$.complete(); },
  //       _ => { decision$.next(false); decision$.complete(); }
  //     );

  //     // return the subject that will eventually emit the user's decision
  //     return decision$;

  //   } else {

  //     // IF there are no unsaved changes, the navigation can happily proceed
  //     return true;
  //   }
  // }

  // public displayModalError(errorMessage: string) {
  //   // shows the error message in a dismissable modal
  //   this._modalErrorMessage = errorMessage;
  //   this.modalService.open(this.errorModal);
  // }

  get viewModel(): DtoForSaveKeyBase {
    // view data is always directly referencing the global workspace
    // this way, un update to a record in the global workspace automatically
    // updates all places where this record is displayed... nifty
    const s = this.state;
    return !!s.detailsId ? this.workspace.current[this.collection][s.detailsId] : null;
  }

  public handleActionError(friendlyError: any) {

    // This handles any errors caused by actions
    if (friendlyError.status === 422) {
      const keys = Object.keys(friendlyError.error);
      keys.forEach(key => {
        // most validation error keys are expected to start with '[0].'
        // the code below removes this prefix
        let modifiedKey: string;
        const prefix = '[0].';
        if (key.startsWith(prefix)) {
          modifiedKey = key.substring(prefix.length);
        } else {
          modifiedKey = key;
        }

        this._validationErrors[modifiedKey] = friendlyError.error[key];
      });

    } else {
      // TODO
      // this.displayModalError(friendlyError.error);
    }
  }

  ////// UI Bindings

  get errorMessage() {
    return this._errorMessage;
  }

  get modalErrorMessage() {
    return this._modalErrorMessage;
  }

  get validationErrors() {
    return this._validationErrors;
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
    this.router.navigate(['..', 'new'], { relativeTo: this.route });
  }

  get canCreate(): boolean {
    return true; // TODO !this.canUpdatePred || this.canUpdatePred();
  }

  onClone(): void {
    const params: Params = {
      cloneId: this.activeModel.Id.toString()
    };

    this.router.navigate(['..', 'new', params], { relativeTo: this.route });
  }

  get canClone(): boolean {
    return !!this.activeModel && !!this.activeModel;
  }

  onEdit(): void {
    if (this.viewModel) {
      // clone the model (to allow for canceling changes)
      this._viewModelJson = JSON.stringify(this.viewModel);
      this._editModel = JSON.parse(this._viewModelJson);

      // show the edit view
      this.state.detailsStatus = DetailsStatus.edit;
    }
  }

  get canEdit(): boolean {
    // TODO  (!this.canUpdatePred || this.canUpdatePred()) && (this.activeModel && this.enableEditButtonPred(this.activeModel));
    return !!this.activeModel;
  }

  onSave(): void {
    if (!this.isDirty && !this.isNew) {
      // since no changes, don't save to the database
      // just go back to view mode
      this.clearErrors();
      this._editModel = null;
      this.state.detailsStatus = DetailsStatus.loaded;
    } else {

      // clear any errors displayed
      this.clearErrors();

      // we need the original value for when the save API call returns
      const isNew = this.isNew;

      // TODO: some screens may wish to customize this behavior for e.g. line item DTOs
      this._editModel.EntityState = isNew ? 'Inserted' : 'Updated';

      // prepare the save observable
      this.crud.save([this._editModel], { expand: this.expand, returnEntities: true }).subscribe(
        (response: EntitiesResponse) => {

          // update the workspace with the DTO from the server
          const s = this.state;
          s.detailsId = addToWorkspace(response, this.workspace)[0];

          // IF it's a new entity add it to the global state, (not the local one one even if inside a popup)
          if (isNew) {
            this.globalState.insert([s.detailsId]);
            this.router.navigate(['..', s.detailsId], { relativeTo: this.route });
          }

          // in screen mode always close the edit view
          s.detailsStatus = DetailsStatus.loaded;

          // remove the local copy the user was editing
          this._editModel = null;
        },
        (friendlyError) => this.handleActionError(friendlyError)
      );
    }
  }

  onCancel(): void {
    // in screen mode...
    // remove the edit model
    if (this.isNew) {

      // this step in order to avoid the unsaved changes modal
      this.state.detailsStatus = DetailsStatus.loaded;

      // navigate back to the last screen
      // TODO
      // this.location.back();

    } else {
      // clear the edit model and error messages
      this._editModel = null;
      this.clearErrors();

      // ... and then close the edit form
      this.state.detailsStatus = DetailsStatus.loaded;
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

  get canDelete(): boolean {
    // TODO && (!this.canUpdatePred || this.canUpdatePred());
    return !!this.viewModel;
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

}
