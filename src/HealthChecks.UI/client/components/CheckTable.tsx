import React, { Component, FunctionComponent, useEffect, useState } from 'react';
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
    
    useEffect(() => {
        let interval  = 0;
        if(!isOpenPanel) {
            interval = setTimeout(() => {
                setSelectedHealthcheck(null);
                setSelectedHistory(null);
            }, 200);
        }

        return () => {
            if(interval != 0) clearInterval(interval);
        };

    },[isOpenPanel]);

    const renderTable = () => {

        return !Array.isArray(checks) ? (
            <tr>
                <td colSpan={5}>{checks}</td>
            </tr>
        ) : (                
                checks.map((item, index) => {
                    
                    let tags = null;

                    if(item.tags) {                        
                        tags = item.tags.map(t => <span className="tag">{t}</span>);
                    }

                    return (
                        <tr key={index}>
                            <td>{item.name}</td>
                            <td>{tags}</td>
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

    return (
        <>
            <table className="hc-checks-table">
                <thead className="hc-checks-table__header">
                    <tr>
                        <th style={{ width: "20%" }}>Name</th>
                        <th style={{ width: "10%" }}>Tags</th>
                        <th style={{ width: "10%" }}>Health</th>
                        <th style={{ width: "30%" }}>Description</th>
                        <th style={{ width: "20%" }}>Duration</th>
                        <th style={{ width: "10%" }}>Details</th>
                    </tr>
                </thead>
                <tbody className="hc-checks-table__body">{renderTable()}</tbody>
            </table>
            <LivenessPanel
                visible={isOpenPanel}
                onClosePanel={() => setOpenPanel(false)}>
                <LivenessDetail
                    healthcheck={selectedHealthcheck!}
                    executionHistory={selectedHistory!}>
                </LivenessDetail>
            </LivenessPanel>

        </>
    );
}

export { CheckTable };