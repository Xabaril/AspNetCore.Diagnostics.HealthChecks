import React, { FunctionComponent } from 'react';
import { Liveness, CustomGrouping } from '../typings/models';
import { discoveryServices, getStatusConfig } from '../healthChecksResources';
import { CheckTable } from './CheckTable';

interface LivenessTableProps {
  livenessData: Array<Liveness>;
  collapseAll: (event: any) => void;
  expandAll: (event: any) => void;
}

const LivenessTable: FunctionComponent<LivenessTableProps> = ({ livenessData, expandAll, collapseAll }) => {

  const mapTable = (livenessData: any): any[] => {
    let groups: string[] = [];
    return [livenessData.map((el: any) => {
      groups.push(el.status);
      let basis: Liveness[] = el.executions;
      return basis.map(
        (liveness: Liveness) => {
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
        }
      );
    }), groups];
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

  const[groupedEntries, groupHealthList] = mapTable(livenessData);
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
        {groupedEntries.map((group: any, index: any) => {
          const statusConfigGroupLvl = getStatusConfig(groupHealthList[index]);
          return group.map((item: any, indx: any) => {
            const statusConfig = getStatusConfig(item.status);
            const isDefaultGroup = !item.group;
            item.group = item.group ? item.group : "Default";
            return (
              <React.Fragment key={index}>
                {indx === 0 && !isDefaultGroup ? <tr className="groupRow" onClick={e => toggleGroupDetails(e)}>
                  <td className="align-center">
                    <button title="expand group" className="groupButton">
                      <i className="material-icons js-toggle-event">
                        add
                      </i>
                    </button>
                  </td>
                  <td className="align-center"><b>{item.group}</b></td>
                  <td className="align-center">
                    <i className="material-icons"
                       style={{
                         paddingRight: '0.5rem',
                         color: `var(${statusConfigGroupLvl!.color})`
                       }}>
                       {statusConfigGroupLvl!.image}
                    </i>
                  </td>
                  <td>{item.onStateFrom}</td>
                  <td className="align-center">
                    {new Date(item.lastExecuted).toLocaleString()}
                  </td>
                </tr> : null}

                <tr
                  className={ isDefaultGroup ? "hc-table__row " + item.group : "hc-table__row is-hidden" + " " + item.group}
                  onClick={toggleVisibility}>
                  <td className="align-center">
                    <i
                      className={"material-icons js-toggle-event" + " " + item.group + "Icon"}
                      title="expand info">
                      add
                    </i>
                  </td>
                  <td className="align-center">
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
                <tr className={"hc-checks-table-container is-hidden" + " " + item.group + "Details"}>
                  <td colSpan={5}>
                    <CheckTable checks={item.entries} history={item.history} />
                  </td>
                </tr>
              </React.Fragment>
            );
          })
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

const toggleGroupDetails = (event: any) => {
  const mainElement = event.currentTarget;
  const clickedGroupName = mainElement.getElementsByTagName("td")[1].innerText;
  const groupElementsToBeToggled = document.getElementsByClassName(clickedGroupName);
  const detailsTables = document.getElementsByClassName(clickedGroupName + "Details");
  const subGroupIconsToBeToggled = document.getElementsByClassName(clickedGroupName + "Icon");
  const isHidden = groupElementsToBeToggled[0].classList.contains('is-hidden');

  if (isHidden) {
    showElements(groupElementsToBeToggled);
  } else {
    hideElements(groupElementsToBeToggled);
    hideElements(detailsTables);
  }

  updateGroupLvlIcon(event, isHidden);
  updateSubGroupLvlIcons(!isHidden, subGroupIconsToBeToggled);

  function showElements(domElements: HTMLCollection) {
    for (let item of domElements) {
      item.classList.remove('is-hidden');
    }
  }

  function hideElements(domElements: HTMLCollection) {
    for (let item of domElements) {
      item.classList.add('is-hidden');
    }
  }

  function updateGroupLvlIcon(event: any, isHidden: boolean) {
    const { currentTarget } = event;
    const iconImage = currentTarget.getElementsByClassName('js-toggle-event')[0];
    iconImage.innerHTML = isHidden ? 'remove' : 'add';
    iconImage.setAttribute('title', isHidden ? 'hide info' : 'expand info');
  }

  function updateSubGroupLvlIcons(isHidden: boolean, iconsFromDOM: HTMLCollection) {
    for (let icon of iconsFromDOM) {
      icon.innerHTML = isHidden ? 'remove' : 'add';
      icon.setAttribute('title', isHidden ? 'hide info' : 'expand info');
    }
  }
}

export { LivenessTable };
