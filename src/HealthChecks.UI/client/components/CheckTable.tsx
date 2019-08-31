import React from 'react';
import { Check } from '../typings/models';
import { getStatusConfig } from '../healthChecksResources';

interface CheckTableProps {
  checks: Array<Check>;
}

const renderTable = (props: CheckTableProps) => {
  if (!Array.isArray(props.checks)) {
    return (
      <tr>
        <td colSpan={4}>{props.checks}</td>
      </tr>
    );
  }

  return props.checks.map((item, index) => {
    const statusConfig = getStatusConfig(item.status);

    return (
      <tr key={index}>
        <td>{item.name}</td>
        <td>
          <i
            className="material-icons"
            style={{
              paddingRight: '0.5rem',
              color: `var(${statusConfig!.color})`
            }}>
            {statusConfig!.image}
          </i>
          {item.status}
        </td>
        <td>{item.description}</td>
        <td className="align-center">{item.duration.toString()}</td>
      </tr>
    );
  });
};

const CheckTable: React.SFC<CheckTableProps> = props => {
  return (
    <table className="hc-checks-table">
      <thead className="hc-checks-table__header">
        <tr>
          <th>Name</th>
          <th>Health</th>
          <th>Description</th>
          <th>Duration</th>
        </tr>
      </thead>
      <tbody className="hc-checks-table__body">{renderTable(props)}</tbody>
    </table>
  );
};

export { CheckTable };
