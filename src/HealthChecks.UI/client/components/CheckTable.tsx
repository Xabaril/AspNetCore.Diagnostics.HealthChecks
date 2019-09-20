import React, { Component } from 'react';
import { Check, ExecutionHistory } from '../typings/models';
import { getStatusConfig } from '../healthChecksResources';
import { LivenessDetail } from './LivenessDetail';
import { LivenessPage } from './LivenessPage';
import LivenessPanel from './LivenessPanel';

interface CheckTableProps {
  checks: Array<Check>;
  history: Array<ExecutionHistory>;
}

interface CheckTableState {
  isOpenPanel: boolean;
}
export class CheckTable extends Component<CheckTableProps, CheckTableState> {
  constructor(props: CheckTableProps) {
    super(props);
    this.state = { isOpenPanel: false };

    this.renderTable = this.renderTable.bind(this);
  }

  renderTable = () => {
    const props = this.props;
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
          <td>
            <button
              className="hc-action-btn"
              onClick={() => this.setState({ isOpenPanel: true })}>
              <i className="material-icons">history</i>
            </button>
            <LivenessPanel
              onClosePanel={() => this.setState({ isOpenPanel: false })}
              isOpen={this.state.isOpenPanel}>
              <LivenessDetail
                healthcheck={item}
                executionHistory={props.history}></LivenessDetail>
            </LivenessPanel>
          </td>
        </tr>
      );
    });
  };
  render() {
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
        <tbody className="hc-checks-table__body">{this.renderTable()}</tbody>
      </table>
    );
  }
}
