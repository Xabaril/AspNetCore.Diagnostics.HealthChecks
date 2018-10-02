import * as React from "react";
import * as ReactDOM from "react-dom";
import { App } from './App';
import { BrowserRouter } from "react-router-dom";

declare var uiEndpoint: any;
declare var webhookEndpoint: any

let endpoint = `${window.location.origin}${uiEndpoint}`;

ReactDOM.render(
    <BrowserRouter>
        <App apiEndpoint={uiEndpoint}
             webhookEndpoint={webhookEndpoint}
             mountPath={window.location.pathname} />
    </BrowserRouter>, document.getElementById("app"));