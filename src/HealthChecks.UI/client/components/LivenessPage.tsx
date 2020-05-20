import * as React from 'react';
import { HealthChecksClient } from '../healthChecksClient';
import moment from 'moment';
import { Liveness } from '../typings/models';
import { LivenessTable } from './LivenessTable';
const DarkHeartIcon = require('../../assets/svg/dark-heart.svg');
const ExpandIcon = require('../../assets/svg/expand.svg');
const CollapseIcon = require('../../assets/svg/collapse.svg');
const PlusIcon = require('../../assets/svg/plus.svg');
const MinusIcon = require('../../assets/svg/minus.svg');

const healthChecksIntervalStorageKey = 'healthchecks-ui-polling';

interface LivenessState {
    error: Nullable<string>;
    pollingIntervalSetting: string | number;
    livenessData: Array<Liveness>;
}

interface LivenessProps {
    endpoint: string;
}

export class LivenessPage extends React.Component<
    LivenessProps,
    LivenessState
    > {
    private _healthChecksClient: HealthChecksClient;
    private _lifenessTable: any;
    constructor(props: LivenessProps) {
        super(props);
        this._healthChecksClient = new HealthChecksClient(this.props.endpoint);
        this.initPolling = this.initPolling.bind(this);
        this.onPollinIntervalChange = this.onPollinIntervalChange.bind(this);
        this.expandAll = this.expandAll.bind(this);
        this.collapseAll = this.collapseAll.bind(this);

        const pollingIntervalSetting =
            localStorage.getItem(healthChecksIntervalStorageKey) || 10;

        this.state = {
            error: '',
            livenessData: [],
            pollingIntervalSetting
        };
    }

    componentDidMount() {
        this.load();
        this.initPolling();
    }

    async load() {
        try {
            let livenessCollection = (await this._healthChecksClient.getData())
                .data as Array<Liveness>;
            livenessCollection = livenessCollection.filter(l => l != null);

            for (let item of livenessCollection) {
                item.onStateFrom = `${item.status} ${moment
                    .utc(item.onStateFrom)
                    .fromNow()}`;
            }

            this.setState({
                livenessData: livenessCollection
            });

        } catch (error) {
            this.setState({
                error: 'Could not retrieve health checks data'
            });
            console.error(error);
        }
    }

    initPolling() {
        localStorage.setItem(
            healthChecksIntervalStorageKey,
            this.state.pollingIntervalSetting.toString()
        );
        this._healthChecksClient.startPolling(
            this.configuredInterval(),
            this.onPollingElapsed.bind(this)
        );
    }

    configuredInterval(): string | number {
        let configuredInterval =
            localStorage.getItem(healthChecksIntervalStorageKey) ||
            this.state.pollingIntervalSetting;
        return (configuredInterval as any) * 1000;
    }

    onPollingElapsed() {
        this.setState({ error: '' });
        this.load();
    }

    onPollinIntervalChange(event: any) {
        this.setState({
            pollingIntervalSetting: event.target.value
        });
    }

    componentWillUnmount() {
        this._healthChecksClient.stopPolling();
    }

    expandAll(event: any) {
        var tableElement = this._lifenessTable;
        Array.from(
            tableElement.getElementsByClassName('hc-checks-table-container')
        ).forEach((el: any) => el.classList.remove('is-hidden'));

        Array.from(tableElement.getElementsByClassName('js-toggle-event')).forEach(
            (el: any) => {
                el.innerHTML = 'remove';
                el.setAttribute('title', 'hide info');
            }
        );
    }

    collapseAll(event: any) {
        var tableElement = this._lifenessTable;
        Array.from(
            tableElement.getElementsByClassName('hc-checks-table-container')
        ).forEach((el: any) => el.classList.add('is-hidden'));

        Array.from(tableElement.getElementsByClassName('js-toggle-event')).forEach(
            (el: any) => {
                el.innerHTML = 'add';
                el.setAttribute('title', 'expand info');
            }
        );
    }

    render() {
        return (
            <article className="hc-liveness">
                <header className="hc-liveness__header">
                    <h1>Health Checks status</h1>
                    <div className="hc-refesh-group">
                        <span>Refresh every</span>
                        <input
                            value={this.state.pollingIntervalSetting}
                            onChange={this.onPollinIntervalChange}
                            type="number"
                            data-oninput="validity.valid && value > 0 ||(value=10)"
                        />
                        <span>seconds</span>
                        <button
                            onClick={this.initPolling}
                            type="button"
                            className="hc-button">
                            Change
            </button>
                    </div>
                </header>
                <div className="hc-liveness__container">
                    <div
                        className="hc-table-container"
                        ref={lt => (this._lifenessTable = lt)}>
                        <LivenessTable
                            expandAll={this.expandAll}
                            collapseAll={this.collapseAll}
                            livenessData={this.state.livenessData}
                        />
                        {this.state.error ? (
                            <div className="w-100 alert alert-danger" role="alert">
                                {this.state.error}
                            </div>
                        ) : null}
                    </div>
                </div>
            </article>
        );
    }
}
