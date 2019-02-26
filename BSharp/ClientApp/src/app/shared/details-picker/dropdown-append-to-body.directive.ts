import { Directive, forwardRef, Inject, OnDestroy } from '@angular/core';
import { NgbDropdown } from '@ng-bootstrap/ng-bootstrap';
import { Subscription } from 'rxjs';
import { positionElements } from './positioning';

// it's a pity that ng-bootstrap have not implemented a way to attach the
// dropdown to the document body instead of next to the target input, this
// causes the dropdown to get cropped out if the container has a max width
// which is typical for details pickers inside tables for example. This is
// the Github issue that has been open since forever and doesn't seem like
// it will be addressed any time soon despite heavy interest: https://bit.ly/2CxULRs
// to solve this we use a workaround/hack generously provided on the github
// issue by one of the users @EvaSky. One day if the ng-bootstrap team decides
// to introduce offical support for this feature, this this file, together
// with positioning.ts will no longer be needed.

@Directive({
    selector: '[ngbDropdown][bDropdownAppendToBody]'
})
export class DropdownAppendToBodyDirective implements OnDestroy {

    private onChangeSubscription: Subscription;

    constructor(@Inject(forwardRef(() => NgbDropdown)) private dropdown: NgbDropdown) {

        this.onChangeSubscription = this.dropdown.openChange.subscribe((open: boolean) => {
            this.dropdown['_menu'].position = (triggerEl: HTMLElement, placement: string) => {
                if (!this.isInBody()) {
                    this.appendMenuToBody();
                }
                const targetElem = this.dropdown['_menu']['_elementRef'].nativeElement;
                positionElements(triggerEl, targetElem, placement, true);
                targetElem.style.transform = '';
            };

            if (open) {
                if (!this.isInBody()) {
                    this.appendMenuToBody();
                }
            } else {
                setTimeout(() => this.removeMenuFromBody());
            }
        });
    }

    ngOnDestroy() {
        this.removeMenuFromBody();
        if (this.onChangeSubscription) {
            this.onChangeSubscription.unsubscribe();
        }
    }

    private isInBody() {
        return this.dropdown['_menu']['_elementRef'].nativeElement.parentNode === document.body;
    }

    private removeMenuFromBody() {
        if (this.isInBody()) {
            window.document.body.removeChild(this.dropdown['_menu']['_elementRef'].nativeElement);
        }
    }

    private appendMenuToBody() {
        window.document.body.appendChild(this.dropdown['_menu']['_elementRef'].nativeElement);
    }
}
