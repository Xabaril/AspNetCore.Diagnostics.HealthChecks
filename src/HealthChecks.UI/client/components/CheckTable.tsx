import React, { Component } from 'react';
import { Check, ExecutionHistory } from '../typings/models';
import { LivenessDetail } from './LivenessDetail';
import { LivenessPanel } from './LivenessPanel';
import { Status } from './Status';

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
    this.setOpenPanelState = this.setOpenPanelState.bind(this);
  }

  setOpenPanelState() {
    this.setState({ isOpenPanel: !this.state.isOpenPanel });
  }

  renderTable() {
    const props = this.props;
    return !Array.isArray(props.checks) ? (
      <tr>
        <td colSpan={5}>{props.checks}</td>
      </tr>
    ) : (
      props.checks.map((item, index) => {
        return (
          <tr key={index}>
            <td>{item.name}</td>
            <td>
              <Status status={item.status}></Status>
            </td>
            <td>{item.description}</td>
            <td className="align-center">{item.duration.toString()}</td>
            <td>
              <button
                className="hc-action-btn"
                onClick={() => this.setOpenPanelState()}>
                <i className="material-icons">history</i>
              </button>
              <LivenessPanel
                onClosePanel={() => this.setOpenPanelState()}
                isOpen={this.state.isOpenPanel}>
                <LivenessDetail
                  healthcheck={item}
                  executionHistory={props.history}></LivenessDetail>
              </LivenessPanel>
            </td>
          </tr>
        );
      })
    );
  }

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
