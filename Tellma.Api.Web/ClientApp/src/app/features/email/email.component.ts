import { Component, Input, OnChanges, OnDestroy, OnInit, SimpleChanges } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { of } from 'rxjs';
import { catchError, finalize, tap } from 'rxjs/operators';
import { AttachmentPreview, EmailPreview } from '~/app/data/dto/email-command-preview';
import { colorFromExtension, downloadBlob, iconFromExtension } from '~/app/data/util';

@Component({
  selector: 't-email',
  templateUrl: './email.component.html',
  styles: [
  ]
})
export class EmailComponent implements OnInit, OnChanges, OnDestroy {

  blobUrl: string;
  safeBlobUrl: SafeResourceUrl = this.sanitizer.bypassSecurityTrustResourceUrl('about:blank');

  @Input()
  email: EmailPreview;

  constructor(private sanitizer: DomSanitizer) { }

  ngOnInit(): void {
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.email) {
        const prevBody = changes.email.previousValue ? changes.email.previousValue.Body : null;
        const currBody = changes.email.currentValue ? changes.email.currentValue.Body : null;
        if (prevBody !== currBody) {
          if (!!currBody) {
            const blob = new Blob([currBody], { type: 'text/html' });
            this.setBlobUrl(blob);
          } else {
            this.setBlobUrl(null);
          }
        }
      }
  }

  ngOnDestroy(): void {
    this.setBlobUrl(null); // To revoke the url
  }

  private setBlobUrl(blob: Blob) {
    window.URL.revokeObjectURL(this.blobUrl);
    this.blobUrl = !!blob ? window.URL.createObjectURL(blob) + '#toolbar=0&navpanes=0&scrollbar=0' : 'about:blank';
    this.safeBlobUrl = this.sanitizer.bypassSecurityTrustResourceUrl(this.blobUrl);
  }

  private extension(att: AttachmentPreview) {
    if (!att || !att.DownloadName) {
      return null;
    }

    const pieces = att.DownloadName.split('.');
    return pieces[pieces.length - 1];
  }

  public color(att: AttachmentPreview): string {
    const ext = this.extension(att);
    return colorFromExtension(ext);
  }

  public icon(att: AttachmentPreview): string {
    const ext = this.extension(att);
    return iconFromExtension(ext);
  }

  public onViewAttachment(att: AttachmentPreview): void {
    if (!att) {
      return;
    }

    if (!!att.Body) {
      const blob = new Blob([att.Body], { type: 'text/html' });
      downloadBlob(blob, att.DownloadName);
    } else if (!!att.bodyBlob) {
      downloadBlob(att.bodyBlob, att.DownloadName);
    } else if (!!att.bodyResolver) {
      att.isLoading = true;
      delete att.error;
      att.bodyResolver.pipe(
        tap(blob => {
          att.bodyBlob = blob;
          downloadBlob(blob, att.DownloadName);
        }),
        catchError(friendlyError => {
          att.error = friendlyError.error || friendlyError;
          return of();
        }),
        finalize(() => {
          att.isLoading = false;
        }),
      ).subscribe();
    }
  }
}
