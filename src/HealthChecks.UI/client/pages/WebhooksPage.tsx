import React, { FunctionComponent } from 'react';
import { WebHook } from '../typings/models';
import ReactJson from 'react-json-view';
import { chunkArray } from '../utils/array';
import GearIcon from '../../assets/svg/gear.svg';
import { useQuery } from 'react-query';
import fetchers from '../api/fetchers';
import { AlertPanel } from '../components/AlertPanel';


interface WebHooksPageState {
  webhooks: Array<WebHook>;
}

const WebhooksPage = () => {

  const { data: webhooks, isError } = useQuery("webhooks", fetchers.getWebhooks, {retry: 1});  

  const renderWebhooks = (webhooks: Array<WebHook>) => {
    let webHooksChunk = chunkArray(webhooks, 2);
    let components: any[] = [];
    for (let chunkWebhooks of webHooksChunk) {
      const component = (
        <>
          {chunkWebhooks.map((webhook, index) => {
            return (
              <div className="webhook-card" key={index}>
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
    };
    return components;
  }

  return (
    <article className="hc-liveness">
      {isError ? <AlertPanel message="Error fetching webhooks information" /> :
      webhooks !== undefined ?
        <>
          <header className="hc-liveness__header">
            <h1>{webhooks.length} Configured Webhooks</h1>
          </header>
          <div className="hc-liveness__container">
            <div className="hc-webhooks-container">
              {}
              {renderWebhooks(webhooks)}
            </div>
          </div>
        </> : null
      }

    </article>
  );

};

export { WebhooksPage };