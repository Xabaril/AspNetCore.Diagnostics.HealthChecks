import React, { Component, FunctionComponent, useState } from 'react';
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

const CheckTable: FunctionComponent<CheckTableProps> = ({ checks, history }) => {
    const [isOpenPanel, setOpenPanel] = useState<boolean>(false);
    const [selectedHistory, setSelectedHistory] = useState<Nullable<ExecutionHistory[]>>(null);
    const [selectedHealthcheck, setSelectedHealthcheck] = useState<Nullable<Check>>(null);

    const openPanel = (healthCheck: Check, history: Array<ExecutionHistory>) => {
        setOpenPanel(true);
        setSelectedHistory(history);
        setSelectedHealthcheck(healthCheck);
    }

    const closePanel = () => {
        setOpenPanel(false);
        setSelectedHistory(null);
        setSelectedHealthcheck(null);
    }

    const renderTable = () => {

        return !Array.isArray(checks) ? (
            <tr>
                <td colSpan={5}>{checks}</td>
            </tr>
        ) : (
                checks.map((item, index) => {
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
                                    onClick={() => openPanel(item, history.filter(h => h.name == item.name))}>
                                    <i className="material-icons">history</i>
                                </button>
                            </td>
                        </tr>
                    );
                })
            );
    }

    const renderPanel = selectedHealthcheck != null && isOpenPanel;
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
                <tbody className="hc-checks-table__body">{renderTable()}</tbody>
            </table>
            {renderPanel &&
                <LivenessPanel
                    onClosePanel={closePanel}>
                    <LivenessDetail
                        healthcheck={selectedHealthcheck!}
                        executionHistory={selectedHistory!}>
                    </LivenessDetail>
                </LivenessPanel>
            }
        </>
    );
}

export { CheckTable };