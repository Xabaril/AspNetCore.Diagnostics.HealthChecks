import React, { FunctionComponent } from 'react';

interface AlertPanelProps {
    message: string;
}
const AlertPanel: FunctionComponent<AlertPanelProps> = ({ message }) =>
    (
        <div className="alert-panel">
            <i
                className="material-icons"
                style={{
                    paddingRight: '0.5rem',
                    color: `var(--dangerColor)`
                }}>
                error
            </i>
            <span>{message}</span>
        </div>
    );


export { AlertPanel };