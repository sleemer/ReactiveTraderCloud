import { Router,  observeEvent } from 'esp-js';
import { CompositeStatusService } from '../../../services';
import { logger } from '../../../system';
import { ModelBase } from '../../common';
import { ServiceStatusLookup, ApplicationStatusConst, ConnectionType } from '../../../services/model';
import { ConnectionStatus } from '../../../system/service';
import { OpenFin } from '../../../system/openFin';
import _ from 'lodash';

var _log:logger.Logger = logger.create('FooterModel');

export default class FooterModel extends ModelBase {
  _compositeStatusService:CompositeStatusService;
  _openFin:OpenFin;

  isConnectedToBroker:boolean;
  connectionUrl:string;
  connectionType:ConnectionType;
  serviceLookup:ServiceStatusLookup;
  isRunningInOpenFin:boolean;

  constructor(
    modelId:string,
    router:Router,
    compositeStatusService:CompositeStatusService,
    openFin:OpenFin
  ) {
    super(modelId, router);
    this._compositeStatusService = compositeStatusService;
    this._openFin = openFin;

    this.serviceLookup = new ServiceStatusLookup();
    this.isConnectedToBroker = false;
    this.shouldShowServiceStatus = false;
    this.applicationStatus = ApplicationStatusConst.Unknown;
  }

  @observeEvent('init')
  _onInit() {
    _log.info(`Footer model starting`);
    this._subscribeToConnectionStatus();
    this.isRunningInOpenFin = this._openFin && this._openFin.isRunningInOpenFin;
  }

  @observeEvent('toggleServiceStatus')
  _onToggleServiceStatus() {
    _log.debug(`toggling service status`);
    this.shouldShowServiceStatus = !this.shouldShowServiceStatus;
  }


  _subscribeToConnectionStatus() {
    this.addDisposable(
      this._compositeStatusService.serviceStatusStream.subscribeWithRouter(
        this.router,
        this.modelId,
        (serviceStatusLookup:ServiceStatusLookup) => {
          this.serviceLookup = serviceStatusLookup;
        })
    );
    this.addDisposable(
      this._compositeStatusService.connectionStatusStream.subscribeWithRouter(
        this.router,
        this.modelId,
        (connectionStatus:String) => {
          this.isConnectedToBroker = connectionStatus === ConnectionStatus.connected;
          this.connectionUrl = this._compositeStatusService.connectionUrl;
          this.connectionType = this._compositeStatusService.connectionType;
        })
    );
  }

  postProcess() {
    this._updateApplicationStatus();
  }

  openLink(url): void {
    if(this.isRunningInOpenFin){
      this._openFin.openLink(url);
    } else {
      window.open(url, '_blank');
    }
  }

  _updateApplicationStatus() {
    const services = Object.keys(this.serviceLookup.services)
                           .map(key => this.serviceLookup.services[key]);
    if (this.isConnectedToBroker && _.every(services, 'isConnected') && this.connectionType === ConnectionType.WebSocket) {
      this.applicationStatus = ApplicationStatusConst.Healthy;
    } else if (_.some(services, 'isConnected')) {
      this.applicationStatus = ApplicationStatusConst.Warning;
    } else {
      this.applicationStatus = ApplicationStatusConst.Down;
    }
  }
}
