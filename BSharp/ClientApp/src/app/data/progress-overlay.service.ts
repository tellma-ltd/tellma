import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ProgressOverlayService {

  private _labels: string[];
  private _oldProgress: { key: string, labelName?: string }[];

  constructor() { }

  private progress: { key: string, labelName?: string }[] = [
    // { key: 'S', labelName: 'LoadingTranslations' },
    // { key: 'O', labelName: 'LoadingSystemSettings' }
  ];

  startAsyncOperation(key: string, labelName?: string) {
    const copy = this.progress.slice();
    copy.push({ key: key, labelName: labelName });
    this.progress = copy;
  }

  completeAsyncOperation(key: string) {
    this.progress = this.progress.filter(e => e.key !== key);
  }

  get asyncOperationInProgress(): boolean {
    return this.progress.length > 0;
  }

  get labelNames(): string[] {

    if (this._oldProgress !== this.progress) {
      this._oldProgress = this.progress;
      this._labels = this.progress.map(e => e.labelName);
    }

    return this._labels;
  }
}
