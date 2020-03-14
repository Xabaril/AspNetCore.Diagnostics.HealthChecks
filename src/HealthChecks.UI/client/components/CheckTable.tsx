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
    selectedHistory: Nullable<Array<ExecutionHistory>>;
    selectedHealthcheck: Nullable<Check>;
}
export class CheckTable extends Component<CheckTableProps, CheckTableState> {
    constructor(props: CheckTableProps) {
        super(props);
        this.state = {
            isOpenPanel: false,
            selectedHistory: null,
            selectedHealthcheck: null
        };

        this.renderTable = this.renderTable.bind(this);
        this.openPanel = this.openPanel.bind(this);
        this.closePanel = this.closePanel.bind(this);
    }

    openPanel(healthCheck: Check, history: Array<ExecutionHistory>) {
        this.setState(
            {
                isOpenPanel: true,
                selectedHistory: history,
                selectedHealthcheck: healthCheck
            });
    }

    closePanel() {
        this.setState(
            {
                isOpenPanel: false,
                selectedHealthcheck: null,
                selectedHistory: null
            });
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
                                    onClick={() => this.openPanel(item, props.history.filter(h => h.name == item.name))}>
                                    <i className="material-icons">history</i>
                                </button>
                            </td>
                        </tr>
                    );
                })
            );
    }

    render() {
        const renderPanel = this.state.selectedHealthcheck != null &&
            this.state.isOpenPanel;
        return (
            <>
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
                {renderPanel &&
                    <LivenessPanel
                        onClosePanel={this.closePanel}>
                        <LivenessDetail
                            healthcheck={this.state.selectedHealthcheck!}
                            executionHistory={this.state.selectedHistory!}>
                        </LivenessDetail>
                    </LivenessPanel>
                }
            </>
        );
    }
}
