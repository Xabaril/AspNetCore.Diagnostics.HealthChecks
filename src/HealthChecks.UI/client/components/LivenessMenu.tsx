import React, { FunctionComponent, useState } from "react";


interface LivenessMenuProps {
   running: boolean
   onRunningClick: () => void;
}

const LivenessMenu : FunctionComponent<LivenessMenuProps> = ({running, onRunningClick}) => {
    return (
    <div className="hc-refesh-group">

        <button
            onClick={onRunningClick}
            type="button"
            className="hc-button">
            {running ? "Stop" : "Start"}
        </button>
    </div>)
};

export {LivenessMenu};