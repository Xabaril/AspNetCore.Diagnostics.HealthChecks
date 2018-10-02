import React from "react";
import { HealthChecksClient } from "../healthChecksClient";
import { WebHook } from "../typings/models";
import ReactJson from "react-json-view";
import { chunkArray } from "../utils/array";
interface WebhooksPageProps {
    endpoint: string;
}
interface WebHooksPageState {
    webhooks: Array<WebHook>;
}
const GearIcon = require('../../assets/svg/gear.svg');

export class WebhooksPage extends React.Component<WebhooksPageProps, WebHooksPageState> {
    private _healthChecksClient: HealthChecksClient;
    constructor(props: WebhooksPageProps) {
        super(props);
        this._healthChecksClient = new HealthChecksClient(props.endpoint);
        this.state = {
            webhooks: []
        }
    }
    componentDidMount() {
        this.getWebhooks();
    }

    async getWebhooks() {
        const webhooks = (await this._healthChecksClient.getData()).data as Array<WebHook>;
        this.setState({
            webhooks
        });
    }
    renderWebhooks(webhooks: Array<WebHook>) {
        let webHooksChunk = chunkArray(webhooks, 2);
        let components: any[] = [];
        for (let chunkWebhooks of webHooksChunk) {
            var component = <div className="row">
                {chunkWebhooks.map((webhook, index) => {
                    return <div className="col-md-6">
                        <div className="webhook">
                            <div className="content">
                                <div>
                                    <span className="block "><b>Name</b> : {webhook.name}</span>
                                </div>
                                <div>
                                    <span className="block break-word"><b>Uri</b> : {webhook.uri}</span>
                                </div>
                                <div>
                                    <span className="block"><b>Payload</b> :</span>
                                    <ReactJson src={webhook.payload as Object} />
                                </div>
                            </div>
                        </div>
                    </div>
                })}
            </div>
            components.push(component);
        }
        return components;
    }
    render() {
        return <div id="wrapper" style={{ height: '100%', overflow: 'auto' }}>
            <div className="container webhook-container">
                <div className="webhooks">
                    <h2 className="title" style={{ marginLeft: '1.35%', marginTop: '5%' }}>
                        <img className="gear-icon" src={GearIcon} />
                        {this.state.webhooks.length} Configured Webhooks
                    </h2>
                    {this.renderWebhooks(this.state.webhooks)}
                </div>
            </div>
        </div>
    }
}