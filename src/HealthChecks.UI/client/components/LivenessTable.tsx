import React, { FunctionComponent } from 'react';
import { Liveness } from '../typings/models';
import { discoveryServices, getStatusConfig } from '../healthChecksResources';
import { CheckTable } from './CheckTable';


interface LivenessTableProps {
  livenessData: Array<Liveness>;
  collapseAll: (event: any) => void;
  expandAll: (event: any) => void;
}

const LivenessTable: FunctionComponent<LivenessTableProps> = ({ livenessData, expandAll, collapseAll }) => {

  const mapTable = (livenessData: Array<Liveness>): Array<Liveness> => {
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
  };


  const toggleAll = (event: any) => {
    let { currentTarget } = event;
    let iconToggle = currentTarget.getElementsByClassName('js-toggle-all')[0];
    const innerValue = iconToggle.innerHTML;

    if (innerValue == 'add_circle_outline') {
      iconToggle.innerHTML = 'remove_circle_outline';
      currentTarget.setAttribute('title', 'close all');
      return expandAll(event);
    } else {
      iconToggle.innerHTML = 'add_circle_outline';
      currentTarget.setAttribute('title', 'expand all');
      return collapseAll(event);
    }
  }

  return (
    <table className="hc-table">
      <thead className="hc-table__head">
        <tr>
          <th>
            <button title="expand all" onClick={e => toggleAll(e)}>
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
        {mapTable(livenessData).map((item, index) => {
          const statusConfig = getStatusConfig(item.status);
          return (
            <React.Fragment key={index}>
              <tr
                className="hc-table__row"
                onClick={toggleVisibility}>
                <td className="align-center">
                  <i
                    className="material-icons js-toggle-event"
                    title="expand info">
                    add
                  </i>
                </td>
                <td>
                  {getDiscoveryServiceImage(item.discoveryService)}
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
                  {new Date(item.lastExecuted).toLocaleString()}
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
};


const getDiscoveryServiceImage = (discoveryService: string) => {
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

const toggleVisibility = (event: any) => {
  let { currentTarget } = event;
  let checksTable = currentTarget.nextSibling;
  let isHidden = checksTable.classList.contains('is-hidden');
  isHidden
    ? checksTable.classList.remove('is-hidden')
    : checksTable.classList.add('is-hidden');

  let iconImage = currentTarget.getElementsByClassName('js-toggle-event')[0];
  iconImage.innerHTML = isHidden ? 'remove' : 'add';
  iconImage.setAttribute('title', isHidden ? 'hide info' : 'expand info');
};

export { LivenessTable };