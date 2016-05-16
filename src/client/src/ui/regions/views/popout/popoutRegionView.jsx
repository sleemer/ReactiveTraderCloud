import _ from 'lodash';
import React from 'react';
import { ViewBase } from '../../../common';
import { PageContainer } from '../../../common/components';
import { RegionModel, RegionModelRegistration } from '../../model';
import { createViewForModel } from '../../';
import { router } from '../../../../system';
import Popout from './popoutWindow.jsx';

export default class PopoutRegionView extends ViewBase {
  constructor() {
    super();
    this.state = {
      model: null
    };
  }

  render() {
    let model:RegionModel = this.state.model;
    if (model === null) {
      return null;
    }
    let popouts = this._createPopouts(model.modelRegistrations);
    return (
      <div>
        {popouts}
      </div>
    );
  }

  _createPopouts(modelRegistrations:Array<RegionModelRegistration>) {
    return _.map(modelRegistrations, (modelRegistration:RegionModelRegistration) => {
      let view = createViewForModel(modelRegistration.model, modelRegistration.displayContext);
      let width = modelRegistration.regionSettings && modelRegistration.regionSettings.width
        ? modelRegistration.regionSettings.width
        : 400;
      let height = modelRegistration.regionSettings && modelRegistration.regionSettings.height
        ? modelRegistration.regionSettings.height
        : 400;
      let popupAttributes = {
        key: modelRegistration.key,
        url:'/#/popout',
        title:'',
        onClosing: () => this._popoutClosed(this.props.modelId, modelRegistration.model),
        options: {
          width: width,
          height: height,
          resizable: 'no',
          scrollable: 'no'
        }
      };
      return (
        <Popout {...popupAttributes}>
          <PageContainer>
            {view}
          </PageContainer>
        </Popout>
      );
    });
  }

  _popoutClosed(regionModelId, model) {
    router.publishEvent(regionModelId, 'removeFromRegion', {model: model})
  }
}
