import React from "react";
import { Check } from "../typings/models";
import { getStatusImage } from "../healthChecksResources";

interface CheckTableProps {
    checks: Array<Check>;
}

const renderTable = (props: CheckTableProps) => {

    if (!Array.isArray(props.checks)) {
        return <tr>
            <td className="td-message" colSpan={4}>{props.checks}</td></tr>;
    }

    return props.checks.map((item, index) => {
        return <tr key={index} style={{ backgroundColor: '#f6f2f2' }}>
            <td>
                {item.name}
            </td>
            <td>
                <img className="status-icon" src={getStatusImage(item.status)} />
                {item.status}
            </td>
            <td>
                {item.description}
            </td>
            <td className="align-center">
                {item.duration.toString()}
            </td>
        </tr>
    });
}


const CheckTable: React.SFC<CheckTableProps> = (props) => {
    return <table className="hc-checks-table">
        <thead className="hc-checks-table__header">
            <tr>
                <th>Name</th>
                <th>Health</th>
                <th>Description</th>
                <th>Duration</th>
            </tr>
        </thead>
        <tbody className="hc-checks-table__body">
            {renderTable(props)}
        </tbody>
    </table>
}

export { CheckTable };
