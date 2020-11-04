import { Component } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { StatePropDescriptor } from '~/app/data/entities/base/metadata';
import { metadata_SmsMessage, SmsMessageState } from '~/app/data/entities/sms-message';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';


@Component({
  selector: 't-sms-messages-details',
  templateUrl: './sms-messages-details.component.html',
  styles: []
})
export class SmsMessagesDetailsComponent extends DetailsBaseComponent {

  public expand = '';
  private _stateDesc: StatePropDescriptor;

  constructor(private workspace: WorkspaceService, private translate: TranslateService) {
    super();
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  public stateDisplay(state: SmsMessageState) {
    if (!this._stateDesc) {
      const meta = metadata_SmsMessage(null, this.translate);
      this._stateDesc = meta.properties.State as StatePropDescriptor;
    }

    return this._stateDesc.format(state);
  }

  public stateColor(state: SmsMessageState) {
    if (!this._stateDesc) {
      const meta = metadata_SmsMessage(null, this.translate);
      this._stateDesc = meta.properties.State as StatePropDescriptor;
    }

    return this._stateDesc.color(state);
  }
}
