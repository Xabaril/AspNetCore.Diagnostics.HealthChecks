import React, { useState, useRef, useCallback, useEffect } from 'react';
import moment from 'moment';
import { Liveness, UIApiSettings } from '../typings/models';
import { LivenessTable } from '../components/LivenessTable';
import { useQuery } from 'react-query';
import { getHealthChecks } from '../api/fetchers';
import { LivenessMenu } from '../components/LivenessMenu';
import { AlertPanel } from '../components/AlertPanel';


interface LivenessState {
    error: Nullable<string>;
    pollingIntervalSetting: string | number;
    livenessData: Array<Liveness>;
}

interface LivenessProps {
    apiSettings: UIApiSettings;
}

const LivenessPage: React.FunctionComponent<LivenessProps> = ({ apiSettings }) => {

    const tableContainerRef = useRef<HTMLDivElement>(null);
    const [fetchInterval, setFetchInterval] = useState<number | false>(apiSettings.pollingInterval * 1000);
    const [running, setRunning] = useState<boolean>(true);
    
    const { data: livenessData, isError } = useQuery("healthchecks", getHealthChecks,
        { refetchInterval: fetchInterval, keepPreviousData: true, retry: 1 });

    useEffect(() => {
        console.log(`Configured polling interval: ${fetchInterval} milliseconds`);
    }, []);

    useEffect(() => {
        if (!running) {
            setFetchInterval(false);
            return;
        }
        setFetchInterval(apiSettings.pollingInterval * 1000);
    }, [running]);

    const expandAll = useCallback(() => {
        const tableElement = tableContainerRef.current!;
        Array.from(
            tableElement.getElementsByClassName('hc-checks-table-container')
        ).forEach((el: any) => el.classList.remove('is-hidden'));

        Array.from(tableElement.getElementsByClassName('js-toggle-event')).forEach(
            (el: any) => {
                el.innerHTML = 'remove';
                el.setAttribute('title', 'hide info');
            }
        );
    }, [tableContainerRef]);

    const collapseAll = useCallback(() => {
        var tableElement = tableContainerRef.current!;
        Array.from(
            tableElement.getElementsByClassName('hc-checks-table-container')
        ).forEach((el: any) => el.classList.add('is-hidden'));

        Array.from(tableElement.getElementsByClassName('js-toggle-event')).forEach(
            (el: any) => {
                el.innerHTML = 'add';
                el.setAttribute('title', 'expand info');
            });
    }, [tableContainerRef]);

    return (
        <article className="hc-liveness">
            <header className="hc-liveness__header">
                <h1>{apiSettings.headerText}</h1>
                <LivenessMenu
                    pollingInterval={apiSettings.pollingInterval}
                    running={running}
                    onRunningClick={() => setRunning(!running)} />
            </header>
            {isError ? (
                <AlertPanel message="Could not retrieve health checks data" />
            ) : null}
            <div className="hc-liveness__container">
                <div
                    className="hc-table-container"
                    ref={tableContainerRef}>
                    {livenessData !== undefined ? (
                        <LivenessTable
                            expandAll={expandAll}
                            collapseAll={collapseAll}
                            livenessData={livenessData!}
                        />) : null}
                </div>
            </div>
        </article>
    );

};

export { LivenessPage };