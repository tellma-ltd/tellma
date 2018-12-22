import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { BrandComponent } from './brand/brand.component';
import { NgbDropdownModule, NgbModalModule, NgbCollapseModule, NgbPopoverModule } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule } from '@ngx-translate/core';
import { MasterComponent } from './master/master.component';
import { DetailsComponent } from './details/details.component';
import { HttpClientModule } from '@angular/common/http';

@NgModule({
  declarations: [
    BrandComponent,
    MasterComponent,
    DetailsComponent
  ],
  imports: [
    CommonModule,
    TranslateModule
  ],
  exports: [
    CommonModule,
    HttpClientModule,
    BrandComponent,
    MasterComponent,
    NgbDropdownModule,
    NgbModalModule,
    NgbCollapseModule,
    NgbPopoverModule,
    TranslateModule
  ]
})
export class SharedModule { }
