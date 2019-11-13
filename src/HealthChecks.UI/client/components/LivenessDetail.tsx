import React from 'react';
import {
  VerticalTimeline,
  VerticalTimelineElement
} from 'react-vertical-timeline-component';
import 'react-vertical-timeline-component/style.min.css';
import { ExecutionHistory, Check } from '../typings/models';
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
        <Status status={props.healthcheck.status}></Status>
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
                  <p>
                          <Status status={e.status}></Status>
                          <span title={e.description}>
                              {e.description &&
                               e.description.length > 100 ?
                              `${e.description.substring(0, 100)}...` : e.description}</span>
                  </p>
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
