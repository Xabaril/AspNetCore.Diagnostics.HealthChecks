import React from 'react';
import { HealthChecksClient } from '../healthChecksClient';
import { WebHook } from '../typings/models';
import ReactJson from 'react-json-view';
import { chunkArray } from '../utils/array';
interface WebhooksPageProps {
  endpoint: string;
}
interface WebHooksPageState {
  webhooks: Array<WebHook>;
}
const GearIcon = require('../../assets/svg/gear.svg');

export class WebhooksPage extends React.Component<
  WebhooksPageProps,
  WebHooksPageState
> {
  private _healthChecksClient: HealthChecksClient;
  constructor(props: WebhooksPageProps) {
    super(props);
    this._healthChecksClient = new HealthChecksClient(props.endpoint);
    this.state = {
      webhooks: []
    };
  }
  componentDidMount() {
    this.getWebhooks();
  }

  async getWebhooks() {
    const webhooks = (await this._healthChecksClient.getData()).data as Array<
      WebHook
    >;
    this.setState({
      webhooks
    });
  }
  renderWebhooks(webhooks: Array<WebHook>) {
    let webHooksChunk = chunkArray(webhooks, 2);
    let components: any[] = [];
    for (let chunkWebhooks of webHooksChunk) {
      var component = (
        <>
          {chunkWebhooks.map((webhook, index) => {
            return (
              <div className="webhook-card">
                <p>
                  <b>Name</b>: {webhook.name}
                </p>
                <p>
                  <b>Payload</b> :
                </p>
                <ReactJson src={webhook.payload as Object} />
              </div>
            );
          })}
        </>
      );
      components.push(component);
    }
    return components;
  }
  render() {
    return (
      <article className="hc-liveness">
        <header className="hc-liveness__header">
          <h1>{this.state.webhooks.length} Configured Webhooks</h1>
        </header>
        <div className="hc-liveness__container">
          <div className="hc-webhooks-container">
            {this.renderWebhooks(this.state.webhooks)}
          </div>
        </div>
      </article>
    );
  }
}
