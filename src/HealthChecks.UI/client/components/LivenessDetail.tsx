import React from 'react';
import {
  VerticalTimeline,
  VerticalTimelineElement
} from 'react-vertical-timeline-component';
import 'react-vertical-timeline-component/style.min.css';
import { ExecutionHistory, Check } from '../typings/models';
import { getStatusConfig } from '../healthChecksResources';
import { Status } from './Status';
import moment from 'moment';

interface LivenessDetailsProps {
  healthcheck: Check;
  executionHistory: Array<ExecutionHistory>;
}

const LivenessDetail: React.SFC<LivenessDetailsProps> = props => {
  return (
    <section className="hc-liveness-detail">
      <header>
        <h2>{props.healthcheck.name}</h2>
        <h3 className="">
          <Status status={props.healthcheck.status}></Status>
        </h3>
      </header>
      <div className="hc-liveness-detail__body">
        {props.executionHistory.length > 0 && (
          <VerticalTimeline className="hc-timeline">
            {' '}
            {props.executionHistory.reverse().map(e => {
              return (
                <VerticalTimelineElement
                  className="hc-timeline__event"
                  date={moment(e.on)
                    .format('LLL')
                    .toString()}>
                  <h3>
                    <Status status={e.status}></Status>
                  </h3>
                </VerticalTimelineElement>
              );
            })}
          </VerticalTimeline>
        )}
      </div>
    </section>
  );
};

export { LivenessDetail };
