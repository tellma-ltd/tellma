import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { WidgetsRoutingModule } from './widgets-routing.module';
import { BrandComponent } from './brand/brand.component';
import { NgbDropdownModule, NgbModalModule, NgbCollapseModule, NgbPopoverModule } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule } from '@ngx-translate/core';

@NgModule({
  declarations: [
    BrandComponent
  ],
  imports: [
    CommonModule,
    WidgetsRoutingModule,
    TranslateModule
  ],
  exports: [
    BrandComponent,
    NgbDropdownModule,
    NgbModalModule,
    NgbCollapseModule,
    NgbPopoverModule,
    TranslateModule
  ]
})
export class WidgetsModule { }
