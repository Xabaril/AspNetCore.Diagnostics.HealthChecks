import "../assets/material.css";
import * as React from "react";
import * as ReactDOM from "react-dom";
import { App } from './App';
import { HashRouter } from "react-router-dom";
import uiSettings from "./config/UISettings";
import { AuthenticationProvider } from "./auth/AuthenticationProvider";


let endpoint = `${window.location.origin}${window.uiEndpoint}`;

ReactDOM.render(
    <HashRouter>
        <AuthenticationProvider>
            <App uiSettings={uiSettings} />
        </AuthenticationProvider>
    </HashRouter>, document.getElementById("app"));