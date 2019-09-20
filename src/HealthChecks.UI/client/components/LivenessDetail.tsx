import React from 'react';
import {
  VerticalTimeline,
  VerticalTimelineElement
} from 'react-vertical-timeline-component';
import 'react-vertical-timeline-component/style.min.css';
import { ExecutionHistory, Check } from '../typings/models';
import { getStatusConfig } from '../healthChecksResources';
import moment from 'moment';

interface LivenessDetailsProps {
  healthcheck: Check;
  executionHistory: Array<ExecutionHistory>;
}

const LivenessDetail: React.SFC<LivenessDetailsProps> = props => {
  return (
    <>
      <p>{props.healthcheck.name}</p>
      <p>Current status: {props.healthcheck.status}</p>

      {props.executionHistory.length > 0 && (
        <VerticalTimeline>
          {' '}
          {props.executionHistory.reverse().map(e => {
            const statusConfig = getStatusConfig(e.status);
            return (
              <VerticalTimelineElement
                className="vertical-timeline-element--work"
                date={moment(e.on)
                  .format('LLL')
                  .toString()}
                iconStyle={{ background: 'rgb(33, 150, 243)', color: '#fff' }}
                icon="">
                <h3 className="vertical-timeline-element-title">{e.status}</h3>
                <h4 className="vertical-timeline-element-subtitle">
                  <i
                    className="material-icons"
                    style={{
                      paddingRight: '0.5rem',
                      color: `var(${statusConfig!.color})`
                    }}>
                    {statusConfig!.image}
                  </i>
                </h4>
              </VerticalTimelineElement>
            );
          })}
        </VerticalTimeline>
      )}
    </>
  );
};

export { LivenessDetail };
