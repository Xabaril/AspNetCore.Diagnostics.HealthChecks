import React, { FunctionComponent } from "react"
import { Route, Redirect, Switch } from 'react-router-dom';
import { LivenessPage } from '../pages/LivenessPage';
import { WebhooksPage } from '../pages/WebhooksPage';
import { UIApiSettings } from "../typings/models";

interface RoutesProps {
    apiSettings: UIApiSettings;
}

const Routes: FunctionComponent<RoutesProps> = ({apiSettings}) => {       
    return (                
        <Switch>
            <Route
                exact
                path="/"
                render={() => <Redirect to="/healthchecks" />}
            />
            <Route
                path="/healthchecks"
                render={() => <LivenessPage apiSettings={apiSettings} />}
            />
            <Route
                path="/webhooks"
                render={() => (
                    <WebhooksPage />
                )}
            />           
        </Switch>
    )
};

export default Routes;