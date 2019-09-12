import {
  Component, OnInit, Input, OnDestroy, ViewChild,
  ElementRef, OnChanges, SimpleChanges, ChangeDetectionStrategy, ChangeDetectorRef
} from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { Subject, of, Observable, Observer, Subscription } from 'rxjs';
import { switchMap, map, catchError, tap, finalize } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { StorageService } from '~/app/data/storage.service';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { WorkspaceService } from '~/app/data/workspace.service';

enum ImageStatus {
  // The image data is currently being fetched from the server
  loading = 1,

  // The image data is ready for display
  loaded = 2,

  // The last fetch of image data from the server completed with an error
  error = 3,
}

@Component({
  selector: 'b-image',
  templateUrl: './image.component.html',
  styleUrls: ['./image.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: ImageComponent }]
})
export class ImageComponent implements OnInit, OnDestroy, OnChanges, ControlValueAccessor {

  @Input()
  shape: 'square' | 'circle' = 'square';

  @Input()
  icon = 'user';

  @Input()
  size = 90;

  @Input()
  sizeUnit = 'px';

  @Input()
  isEdit = false;

  @Input()
  src: string;

  @Input()
  imageId: string;

  @ViewChild('input', { static: true })
  input: ElementRef;

  @ViewChild('errorModal', { static: true })
  errorModal: ElementRef;

  private _subscription: Subscription;
  private notifyCancel$ = new Subject<void>();
  private notifyUpdate$: Subject<void>;
  private _src: string;
  private _imageId: string;
  private _value: string = null; // base64 byte array
  private _metadataPrefix: string; // base64 byte array

  public status: ImageStatus;
  public dataUrl: string = null;
  public maxSize = 5 * 1024 * 1024;

  ///////////////// Implementation of ControlValueAccessor
  public isDisabled = false;
  public onChange: (val: any) => void = _ => { };
  public onTouched: () => void = () => { };
  public onValidatorChange: () => void = () => { };

  writeValue(v: any): void {

    if (!v && v !== '') {
      v = null;
    }

    if (this._value !== v) {
      this._value = v;
      this._metadataPrefix = 'data:image/jpeg;base64,';
      this.update();
    }
  }

  registerOnChange(fn: (val: any) => void): void {
    this.onChange = (v: any) => {
      fn(v);

      // The image controller behaves the same whether update on input or on blur
      this.onTouched();
    };
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState?(isDisabled: boolean): void {
    this.isDisabled = isDisabled;
  }

  constructor(
    private api: ApiService, private workspace: WorkspaceService, private cdr: ChangeDetectorRef,
    private storage: StorageService, private modalService: NgbModal) {

    this.notifyUpdate$ = new Subject<void>();
    this.notifyUpdate$.pipe(
      switchMap(() => {
        const obs$ = this.doUpdate();
        this.cdr.markForCheck();
        return obs$;
      })
    ).subscribe();
  }

  ngOnInit() {
    this.update();
    this._subscription = this.workspace.stateChanged$.subscribe({
      next: () => {
        this.cdr.markForCheck();
      }
    });
  }

  ngOnDestroy() {
    this.notifyCancel$.next();

    if (!!this._subscription) {
      this._subscription.unsubscribe();
    }
  }

  ngOnChanges(changes: SimpleChanges) {

    // the combinatino of these two properties defines a whole new screen from the POV of the user
    // when either of these properties change it is equivalent to a screen closing and
    // and another screen opening even though Angular may reuse the same
    // component and never call ngOnDestroy and ngOnInit. So we call them
    // manually here if this is not the first time these properties are set
    // to simulate a screen closing and opening again
    const screenDefProperties = [changes.src, changes.imageId];

    const anyChanges = screenDefProperties.some(prop => !!prop);
    const notFirstChange = screenDefProperties.some(prop => !!prop && !prop.isFirstChange());

    if (anyChanges) {

      if (notFirstChange) {
        this.ngOnDestroy();
      }

      if (!!changes.src) {
        this._src = changes.src.currentValue;
      }

      if (!!changes.imageId) {
        this._imageId = changes.imageId.currentValue;
      }

      // set the values
      if (notFirstChange) {
        this.ngOnInit();
      }
    }
  }

  private update(): void {
    this.notifyUpdate$.next();
  }

  private doUpdate(): Observable<void> {

    if (!!this._value) {

      // Shows the value of ngModel
      this.status = ImageStatus.loaded;
      this.dataUrl = this._metadataPrefix + ',' + this._value;
      return of();

    } else if (this._value === '') {

      // Shows the placeholder icon
      this.status = ImageStatus.loaded;
      this.dataUrl = null;
      return of();

    } else if (!this._src || !this._imageId) {

      // this usually indicates that the image was deleted on the server
      if (!!this._src) {
        this.storage.removeItem(`${this.workspace.ws.tenantId}/${this._src}`);
      }

      // If the _value === null, then we rely on apiEndpoint and imageId
      // If the image Id is null or the src is null nothing to load, just show the placeholder icon
      this.status = ImageStatus.loaded;
      this.dataUrl = null;
      return of();

    } else {
      const tenantId = this.workspace.ws.tenantId;
      const src = this._src;
      const imageId = this._imageId;
      const storageKey = `${tenantId}/${src}`;

      const storageItemString = this.storage.getItem(storageKey);
      const storageItem: { imageId: string, dataUrl: string } = !!storageItemString ? JSON.parse(storageItemString) : null;

      if (!!storageItem && storageItem.imageId === imageId) {
        this.status = ImageStatus.loaded;
        this.dataUrl = storageItem.dataUrl;
        return of();

      } else {

        if (!!storageItemString) {
          this.storage.removeItem(storageKey);
        }

        // load the image from the server
        this.status = ImageStatus.loading;
        return this.api.getImage(src, imageId, this.notifyCancel$).pipe(
          switchMap((b: { image: Blob, imageId: string }) => this.getDataURL(b.image).pipe(
            tap((dataUrl: string) => {
              this.status = ImageStatus.loaded;
              this.dataUrl = dataUrl;

              // cache it in local storage for the future
              this.storage.setItem(storageKey, JSON.stringify({ imageId: b.imageId || imageId, dataUrl }));
            }))),
          catchError((error: any) => {
            if (error.status === 404) {
              // 404 means the image was deleted, just show the placeholder
              this.status = ImageStatus.loaded;
              this.dataUrl = null;
            } else {

              // show error
              this.status = ImageStatus.error;
            }
            return of(null);
          }),
          finalize(() => this.cdr.markForCheck())
        );
      }
    }
  }

  private getDataURL(blob: Blob): Observable<string> {
    const reader = new FileReader();
    const obs$ = new Observable<string>((obs: Observer<string>) => {
      // implement the observable contract based on reader
      reader.onloadend = () => {

        const data = reader.result as string;
        obs.next(data);
        obs.complete();
      };

      reader.onerror = (e) => {
        obs.error(e);
      };
    });

    reader.readAsDataURL(blob);
    return obs$;
  }

  // UI Binding

  get showIcon(): boolean {
    return !this.dataUrl && this.status === ImageStatus.loaded;
  }

  get showImg(): boolean {
    return this.dataUrl && this.status === ImageStatus.loaded;
  }

  get showEditControls(): boolean {
    return this.isEdit;
  }

  get showError(): boolean {
    return this.status === ImageStatus.error;
  }

  get isRound(): boolean {
    return this.shape === 'circle';
  }

  get placeholderFontSize(): number {
    return 3.2 * (this.size / 90);
  }

  public onEdit(): void {
    this.input.nativeElement.click();
  }

  public onDelete(): void {
    // An empty bytes array indicates to the
    // server that we wish to delete the image
    if (this.canDelete) {
      this._value = '';
      this.onChange(this._value);
      this.update();
    }
  }

  public get canDelete(): boolean {
    return !!this.dataUrl;
  }

  public onFileSelected(input: any) {
    const files = input.files as FileList;

    if (!files || files.length === 0) {
      return;
    }

    const file = files[0];
    if (file.size > this.maxSize) {
      this.modalService.open(this.errorModal);
      return;
    }

    input.value = '';
    this.getDataURL(file).subscribe(dataUrl => {

      // Get the base64 value from the data URL
      const commaIndex = dataUrl.indexOf(',');
      this._metadataPrefix = dataUrl.substr(0, commaIndex);
      this._value = dataUrl.substr(commaIndex + 1);
      this.onChange(this._value);
      this.update();

    }, (err) => {
      console.error(err);
    });
  }
}
