import React, { FunctionComponent } from 'react';
import { getStatusConfig } from '../healthChecksResources';

interface IStatusProps {
  status: string;
}

const Status: FunctionComponent<IStatusProps> = ({ status }) => {
  const statusConfig = getStatusConfig(status);
  return (
    <div className="hc-status">
      <i
        className="material-icons"
        style={{
          paddingRight: '0.5rem',
          color: `var(${statusConfig!.color})`
        }}>
        {statusConfig!.image}
      </i>
      {status}
    </div>
  );
};

export { Status };
