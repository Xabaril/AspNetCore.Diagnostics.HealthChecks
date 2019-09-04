import React from 'react';
import { Check, ExecutionHistory } from '../typings/models';
import { getStatusConfig } from '../healthChecksResources';
import { LivenessDetail } from './LivenessDetail';


interface CheckTableProps {
  checks: Array<Check>;
  history: Array<ExecutionHistory>;
}

const renderTable = (props: CheckTableProps) => {
  if (!Array.isArray(props.checks)) {
    return (
      <tr>
        <td colSpan={5}>{props.checks}</td>
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
        <td><LivenessDetail healthcheck={item} executionHistory={props.history}></LivenessDetail></td>
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
          <th>Details</th>
        </tr>
      </thead>
      <tbody className="hc-checks-table__body">{renderTable(props)}</tbody>
    </table>
  );
};

export { CheckTable };
