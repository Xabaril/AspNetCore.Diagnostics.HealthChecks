import React, {FunctionComponent, useEffect, useState} from 'react';
import {Check} from '../typings/models';

interface LivenessDataProps {
    healthcheck: Check;
}

const LivenessData: FunctionComponent<LivenessDataProps> = props => {
    if (props.healthcheck === null) return null;

    const [data, setData] = useState(null)
    useEffect(() => {
        setData(JSON.parse(props.healthcheck.data))
    }, [props])

    return (
        <div className="hc-liveness-detail__data">
            {data && (<pre>{JSON.stringify(data, null, 2)}</pre>)}
        </div>
    );
};

export {LivenessData};
