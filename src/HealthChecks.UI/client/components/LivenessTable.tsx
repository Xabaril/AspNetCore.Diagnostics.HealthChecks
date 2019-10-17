import React from 'react';
import { Liveness } from '../typings/models';
import { discoveryServices, getStatusConfig } from '../healthChecksResources';
import { CheckTable } from './CheckTable';


interface LivenessTableProps {
  livenessData: Array<Liveness>;
  collapseAll: (event: any) => void;
  expandAll: (event: any) => void;
}

export class LivenessTable extends React.Component<LivenessTableProps> {
  constructor(props: LivenessTableProps) {
    super(props);
    this.state = {
      livenessData: props.livenessData
    };

    this.mapTable = this.mapTable.bind(this);
    this.toggleAll = this.toggleAll.bind(this);
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

    let iconImage = currentTarget.getElementsByClassName('js-toggle-event')[0];
    iconImage.innerHTML = isHidden ? 'remove' : 'add';
    iconImage.setAttribute('title', isHidden ? 'hide info' : 'expand info');
  }

  toggleAll(event: any) {
    let { currentTarget } = event;
    let iconToggle = currentTarget.getElementsByClassName('js-toggle-all')[0];
    const innerValue = iconToggle.innerHTML;

    if (innerValue == 'add_circle_outline') {
      iconToggle.innerHTML = 'remove_circle_outline';
      currentTarget.setAttribute('title', 'close all');
      return this.props.expandAll(event);
    } else {
      iconToggle.innerHTML = 'add_circle_outline';
      currentTarget.setAttribute('title', 'expand all');
      return this.props.collapseAll(event);
    }
  }

  render() {
    return (
      <table className="hc-table">
        <thead className="hc-table__head">
          <tr>
            <th>
              <button title="expand all" onClick={e => this.toggleAll(e)}>
                <i className="material-icons js-toggle-all">
                  add_circle_outline
                </i>
              </button>
            </th>
            <th>Name</th>
            <th>Health</th>
            <th>On state from</th>
            <th>Last execution</th>
          </tr>
        </thead>
        <tbody className="hc-table__body">
          {this.mapTable(this.props.livenessData).map((item, index) => {
            const statusConfig = getStatusConfig(item.status);
            return (
              <React.Fragment>
                <tr
                  className="hc-table__row"
                  key={index}
                  onClick={this.toggleVisibility}>
                  <td className="align-center">
                    <i
                      className="material-icons js-toggle-event"
                      title="expand info">
                      add
                    </i>
                  </td>
                  <td>
                    {this.getDiscoveryServiceImage(item.discoveryService)}
                    {item.name}
                  </td>
                  <td className="align-center">
                    <i
                      className="material-icons"
                      style={{
                        paddingRight: '0.5rem',
                        color: `var(${statusConfig!.color})`
                      }}>
                      {statusConfig!.image}
                    </i>
                  </td>
                  <td>{item.onStateFrom}</td>
                  <td className="align-center">
                    {this.formatDate(item.lastExecuted)}
                  </td>
                </tr>
                <tr className="hc-checks-table-container is-hidden">
                  <td colSpan={5}>
                    <CheckTable checks={item.entries} history={item.history} />
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
