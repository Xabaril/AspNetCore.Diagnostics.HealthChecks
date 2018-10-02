import React from "react";
import { Liveness, Check } from "../typings/models";
import { getStatusImage, discoveryServices } from "../healthChecksResources";
import { CheckTable } from "./CheckTable";

interface LivenessTableProps {
    livenessData: Array<Liveness>
}

const PlusIcon = require("../../assets/svg/plus.svg");
const MinusIcon = require("../../assets/svg/minus.svg");

export class LivenessTable extends React.Component<LivenessTableProps> {

    constructor(props: LivenessTableProps) {
        super(props);
        this.state = {
            livenessData: props.livenessData
        }

        this.mapTable = this.mapTable.bind(this);
    }

    mapTable(livenessData: Array<Liveness>): Array<Liveness> {

        return livenessData.map(liveness => {
            if (liveness.livenessResult) {
                let checks;
                try {
                    //Check whether liveness result is an string formatted Array or a simple string
                    checks = JSON.parse(liveness.livenessResult).checks;
                    Object.assign(liveness, { checks })
                } catch (err) {
                    Object.assign(liveness, { checks: liveness.livenessResult });
                }
            }
            return liveness;
        });
    }

    formatDate(date: string) {
        return new Date(date).toLocaleString();
    }

    getStatusImage(status: string) {
        return getStatusImage(status);
    }

    getDiscoveryServiceImage(discoveryService: string) {

        if (discoveryService != null) {
            let discoveryServiceImage = discoveryServices.find(ds => ds.name === discoveryService)!.image;
            return <img className="discovery-icon" src={discoveryServiceImage} title="Kubernetes discovered liveness"/>
        }

        return null;
    }

    toggleVisibility(event: any) {
        let { currentTarget } = event;
        let checksTable = currentTarget.nextSibling;
        let isHidden = checksTable.classList.contains("hidden");
        isHidden ?
            checksTable.classList.remove("hidden") :
            checksTable.classList.add("hidden");

        let iconImage = currentTarget.getElementsByClassName("plus-icon")[0];
        iconImage.src = isHidden ? MinusIcon : PlusIcon;
    }

    render() {
        return <div className="table-responsive">
            <table className="table">
                <thead className="thead-dark">
                    <tr>
                        <th></th>
                        <th>Name</th>
                        <th>IsHealthy</th>
                        <th>Status</th>
                        <th>Last Execution</th>
                    </tr>
                </thead>
                <tbody>
                    {this.mapTable(this.props.livenessData).map((item, index) => {
                        return <React.Fragment>
                            <tr className="tr-liveness" key={index} onClick={this.toggleVisibility} style={{ cursor: 'pointer' }}>
                                <td>
                                    <img className="plus-icon" src={PlusIcon} />
                                </td>
                                <td>
                                    {this.getDiscoveryServiceImage(item.discoveryService)}
                                    {item.livenessName}
                                </td>
                                <td className="centered">
                                    <img className="status-icon" src={this.getStatusImage(item.status)} />
                                </td>
                                <td>
                                    {item.onStateFrom}
                                </td>
                                <td>
                                    {this.formatDate(item.lastExecuted)}
                                </td>
                            </tr>
                            <tr className="checks-table hidden">
                                <td style={{ padding: 0 }} colSpan={5}>
                                    <CheckTable checks={item.checks} />
                                </td>
                            </tr>
                        </React.Fragment>
                    })}
                </tbody>
            </table>
        </div>
    }
}

