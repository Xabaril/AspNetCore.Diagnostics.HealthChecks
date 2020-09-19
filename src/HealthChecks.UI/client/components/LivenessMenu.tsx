import { User } from "oidc-client";
import React, { FunctionComponent, useEffect, useState } from "react";

import { useUserStore } from "../stores/userStore";


interface LivenessMenuProps {
    pollingInterval: number,
    running: boolean
    onRunningClick: () => void;
}

const LivenessMenu: FunctionComponent<LivenessMenuProps> = ({ running, onRunningClick, pollingInterval }) => {

    const [user, authService] = useUserStore(state => [state.user, state.authService]);
    const [logout, showLogout] = useState<boolean>(false);

    useEffect(() => {
        if (user) {
            showLogout(true);
        }
    }, [user]);

    return (
        <div className="hc-refesh-group">
            <span style={{display:'block', marginRight: '1.5rem'}}><b>{user ? `Welcome ${user.profile.name}` : null}</b></span>
            <span>Polling interval: <b>{pollingInterval}</b> secs</span>
            <button
                onClick={onRunningClick}
                type="button"
                className="hc-button">
                {running ? "Stop polling" : "Start polling"}
            </button>
            {
                logout ? <button className="hc-button" onClick={() => authService?.logOut()}>Log out</button> : null
            }
        </div>)
};

export { LivenessMenu };