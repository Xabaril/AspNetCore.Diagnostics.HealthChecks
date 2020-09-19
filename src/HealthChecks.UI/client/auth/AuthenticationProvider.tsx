import React, { FunctionComponent, PropsWithChildren } from "react";
import { Route, Switch } from "react-router-dom";
import { AuthCallback } from "./components/AuthCallback";
import { SilentRenewCallback } from "./components/SilentRenewCallback";

export const AuthenticationProvider: FunctionComponent<PropsWithChildren<any>> = ({ children }) => {
    return (
        <Switch>
            <Route exact path="/callback">
                <AuthCallback />
            </Route>
            <Route exact path="/silent-renew-callback">
                <SilentRenewCallback />
            </Route>
            {children}
        </Switch>
    )
}
