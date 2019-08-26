import React from 'react';
import { Liveness } from '../typings/models';
import { getStatusImage, discoveryServices } from '../healthChecksResources';
import { CheckTable } from './CheckTable';

interface LivenessTableProps {
  livenessData: Array<Liveness>;
}

const PlusIcon = require('../../assets/svg/plus.svg');
const MinusIcon = require('../../assets/svg/minus.svg');

export class LivenessTable extends React.Component<LivenessTableProps> {
  constructor(props: LivenessTableProps) {
    super(props);
    this.state = {
      livenessData: props.livenessData
    };

    this.mapTable = this.mapTable.bind(this);
  }

  mapTable(livenessData: Array<Liveness>): Array<Liveness> {
    return livenessData.map(liveness => {
      if (liveness.livenessResult) {
        let checks;
        try {
          //Check whether liveness result is an string formatted Array or a simple string
          checks = JSON.parse(liveness.livenessResult).checks;
          Object.assign(liveness, { checks });
        } catch (err) {
          Object.assign(liveness, { checks: liveness.livenessResult });
        }
      }
      return liveness;
    });
  }

  formatDate(date: string) {
    return new Date(date).toLocaleString();
  }

  getDiscoveryServiceImage(discoveryService: string) {
    if (discoveryService != null) {
      let discoveryServiceImage = discoveryServices.find(
        ds => ds.name === discoveryService
      )!.image;
      return (
        <img
          className="discovery-icon"
          src={discoveryServiceImage}
          title="Kubernetes discovered liveness"
        />
      );
    }

    return null;
  }

  toggleVisibility(event: any) {
    let { currentTarget } = event;
    let checksTable = currentTarget.nextSibling;
    let isHidden = checksTable.classList.contains('is-hidden');
    isHidden
      ? checksTable.classList.remove('is-hidden')
      : checksTable.classList.add('is-hidden');

    let iconImage = currentTarget.getElementsByClassName('plus-icon')[0];
    iconImage.src = isHidden ? MinusIcon : PlusIcon;
  }

  render() {
    return (
      <table className="hc-table">
        <thead className="hc-table__head">
          <tr>
            <th  />
            <th>Name</th>
            <th>Health</th>
            <th>On state from</th>
            <th>Last execution</th>
          </tr>
        </thead>
        <tbody className="hc-table__body">
          {this.mapTable(this.props.livenessData).map((item, index) => {
            return (
              <React.Fragment>
                <tr
                  className="hc-table__row"
                  key={index}
                  onClick={this.toggleVisibility}
                  style={{ cursor: 'pointer' }}>
                  <td >
                    <img className="plus-icon" src={PlusIcon} />
                  </td>
                  <td>
                    {this.getDiscoveryServiceImage(item.discoveryService)}
                    {item.name}
                  </td>
                  <td className="align-center">
                    <img
                      className="status-icon"
                      src={getStatusImage(item.status)}
                    />
                  </td>
                  <td>{item.onStateFrom}</td>
                  <td className="align-center">{this.formatDate(item.lastExecuted)}</td>
                </tr>
                <tr className="hc-checks-table-container is-hidden">
                  <td  colSpan={5}>
                    <CheckTable checks={item.entries} />
                  </td>
                </tr>
              </React.Fragment>
            );
          })}
        </tbody>
      </table>
    );
  }
}
