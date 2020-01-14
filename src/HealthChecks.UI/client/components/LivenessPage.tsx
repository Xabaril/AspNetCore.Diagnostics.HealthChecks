import * as React from 'react';
import { HealthChecksClient } from '../healthChecksClient';
import { PollingInterval, getConfiguredInterval } from "./PollingInterval";
import moment from 'moment';
import { Liveness } from '../typings/models';
import { LivenessTable } from './LivenessTable';
const DarkHeartIcon = require('../../assets/svg/dark-heart.svg');
const ExpandIcon = require('../../assets/svg/expand.svg');
const CollapseIcon = require('../../assets/svg/collapse.svg');
const PlusIcon = require('../../assets/svg/plus.svg');
const MinusIcon = require('../../assets/svg/minus.svg');

interface LivenessState {
  error: Nullable<string>;
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
    this.expandAll = this.expandAll.bind(this);
    this.collapseAll = this.collapseAll.bind(this);

    this.state = {
      error: '',
      livenessData: []
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

  initPolling = () => this._healthChecksClient.startPolling(getConfiguredInterval() * 1000, this.onPollingElapsed);

  onPollingElapsed = () => {
    this.setState({ error: '' });
    this.load();
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
            <PollingInterval onChange={this.initPolling} />
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
