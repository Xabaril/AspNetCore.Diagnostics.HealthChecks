import React from "react";
import { Check } from "../typings/models";

interface CheckTableProps {
    checks: Array<Check>;
}

const renderTable = (props: CheckTableProps) => {

    if (!Array.isArray(props.checks)) {
        return <tr>
            <td className="td-message" colSpan={6}>{props.checks}</td></tr>;
    }

    return props.checks.map((item, index) => {
        return <tr key={index} style={{ backgroundColor: '#f6f2f2' }}>
            <td>
                {item.name}
            </td>
            <td>
                {item.message}
            </td>
            <td>
                {item.elapsed}
            </td>
            <td>
                {item.run.toString()}
            </td>
            <td>
                {item.path}
            </td>
            <td>
                {item.isHealthy.toString()}
            </td>
        </tr>
    });
}


const CheckTable: React.SFC<CheckTableProps> = (props) => {
    return <table className="table-responsive" style={{ display: 'inline-table' }}>
        <thead className="thead-black">
            <tr>
                <th>Name</th>
                <th>Message</th>
                <th>Elapsed time</th>
                <th>Run</th>
                <th>Path</th>
                <th>Is Healthy</th>
            </tr>
        </thead>
        <tbody>
            {renderTable(props)}
        </tbody>
    </table>
}

export { CheckTable };