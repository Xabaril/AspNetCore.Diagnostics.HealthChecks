import "../assets/material.css";
import * as React from "react";
import * as ReactDOM from "react-dom";
import { App } from './App';
import { HashRouter } from "react-router-dom";
import uiSettings from "./config/UISettings";
let endpoint = `${window.location.origin}${window.uiEndpoint}`;

ReactDOM.render(
    <HashRouter>
        <App uiSettings={uiSettings} />
    </HashRouter>, document.getElementById("app"));