import React, { FunctionComponent, useEffect} from "react";

interface LivenessMenuProps {
    pollingInterval: number,
    running: boolean
    onRunningClick: () => void;
}

const LivenessMenu: FunctionComponent<LivenessMenuProps> = ({ running, onRunningClick, pollingInterval }) => {


    return (
        <div className="hc-refesh-group">            
            <span>Polling interval: <b>{pollingInterval}</b> secs</span>
            <button
                onClick={onRunningClick}
                type="button"
                className="hc-button">
                {running ? "Stop polling" : "Start polling"}
            </button>
        </div>)
};

export { LivenessMenu };